import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { useEffect, useRef, useState } from 'react';

export function useGameHub() {
    const connectionRef = useRef<HubConnection | null>(null);
    const [connectionState, setConnectionState] = useState<HubConnectionState>(HubConnectionState.Disconnected);

    useEffect(() => {
        const conn = new HubConnectionBuilder()
            .withUrl('/hub/game', {
                accessTokenFactory: () => sessionStorage.getItem('auth_token') ?? '',
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        conn.onreconnecting(() => setConnectionState(HubConnectionState.Reconnecting));
        conn.onreconnected(() => setConnectionState(HubConnectionState.Connected));
        conn.onclose(() => setConnectionState(HubConnectionState.Disconnected));

        connectionRef.current = conn;

        let cancelled = false;
        conn.start()
            .then(() => { if (!cancelled) setConnectionState(HubConnectionState.Connected); })
            .catch(err => { if (!cancelled) console.error('SignalR connection error:', err); });

        return () => {
            cancelled = true;
            connectionRef.current = null;
            conn.stop();
        };
    }, []);

    return { connectionRef, connectionState };
}
