import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { useEffect, useRef, useState } from 'react';

export function useGameHub() {
    const connectionRef = useRef<HubConnection | null>(null);
    const [connectionState, setConnectionState] = useState<HubConnectionState>(HubConnectionState.Disconnected);

    useEffect(() => {
        let cancelled = false;

        const conn = new HubConnectionBuilder()
            .withUrl('/hub/game', {
                accessTokenFactory: () => sessionStorage.getItem('auth_token') ?? '',
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        conn.onreconnecting(() => setConnectionState(HubConnectionState.Reconnecting));
        conn.onreconnected(() => setConnectionState(HubConnectionState.Connected));
        conn.onclose(() => {
            if (!cancelled) setConnectionState(HubConnectionState.Disconnected);
        });

        connectionRef.current = conn;

        conn.start()
            .then(() => {
                if (cancelled) {
                    conn.stop();
                } else {
                    setConnectionState(HubConnectionState.Connected);
                }
            })
            .catch(err => {
                if (!cancelled) console.error('SignalR connection error:', err);
            });

        return () => {
            cancelled = true;
            connectionRef.current = null;
        };
    }, []);

    return { connectionRef, connectionState };
}
