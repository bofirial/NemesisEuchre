import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { useEffect, useState } from 'react';

export function useGameHub() {
    const [connection] = useState<HubConnection>(() =>
        new HubConnectionBuilder()
            .withUrl('/hub/game', {
                accessTokenFactory: () => sessionStorage.getItem('auth_token') ?? '',
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build()
    );
    const [connectionState, setConnectionState] = useState<HubConnectionState>(HubConnectionState.Disconnected);

    useEffect(() => {
        connection.onreconnecting(() => setConnectionState(HubConnectionState.Reconnecting));
        connection.onreconnected(() => setConnectionState(HubConnectionState.Connected));
        connection.onclose(() => setConnectionState(HubConnectionState.Disconnected));

        connection.start()
            .then(() => setConnectionState(HubConnectionState.Connected))
            .catch(err => console.error('SignalR connection error:', err));

        return () => { connection.stop(); };
    }, [connection]);

    return { connection, connectionState };
}
