import { HubConnectionState } from '@microsoft/signalr';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { GameActive } from '@/components/GameActive';
import { GameLobby } from '@/components/GameLobby';
import { useGameHub } from '@/signalr/useGameHub';
import type { PlayerGameState } from '@/types/game';

export function GamePage() {
    const { sessionName } = useParams<{ sessionName: string }>();
    const { connectionRef, connectionState } = useGameHub();
    const [gameState, setGameState] = useState<PlayerGameState | null>(null);

    useEffect(() => {
        if (connectionState !== HubConnectionState.Connected || !connectionRef.current || !sessionName) return;
        connectionRef.current.invoke<PlayerGameState>('JoinGameAsync', sessionName)
            .then(setGameState)
            .catch(err => console.error('JoinGame failed:', err));
    }, [connectionRef, connectionState, sessionName]);

    if (!gameState) return null;

    return gameState.gameStatus === 'Playing'
        ? <GameActive />
        : <GameLobby />;
}
