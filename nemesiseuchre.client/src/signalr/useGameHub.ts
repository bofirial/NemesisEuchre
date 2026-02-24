import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { useEffect, useState } from 'react';

export function useGameHub() {
    const [connection, setConnection] = useState<HubConnection | null>(null);
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

        setConnection(conn);

        let cancelled = false;
        conn.start()
            .then(() => { if (!cancelled) setConnectionState(HubConnectionState.Connected); })
            .catch(err => { if (!cancelled) console.error('SignalR connection error:', err); });

        return () => {
            cancelled = true;
            setConnection(null);
            conn.stop();
        };
    }, []);

    return { connection, connectionState };
}
