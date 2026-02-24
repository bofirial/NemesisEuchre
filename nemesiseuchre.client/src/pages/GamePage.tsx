import { HubConnectionState } from '@microsoft/signalr';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { GameActive } from '@/components/GameActive';
import { GameLobby } from '@/components/GameLobby';
import { useGameHub } from '@/signalr/useGameHub';
import type { PlayerGameState } from '@/types/game';

export function GamePage() {
    const { gameName } = useParams<{ gameName: string }>();
    const { connection, connectionState } = useGameHub();
    const [gameState, setGameState] = useState<PlayerGameState | null>(null);

    useEffect(() => {
        if (connectionState !== HubConnectionState.Connected || !connection || !gameName) return;
        connection.invoke<PlayerGameState>('JoinGameAsync', gameName)
            .then(setGameState)
            .catch(err => console.error('JoinGame failed:', err));
    }, [connection, connectionState, gameName]);

    if (!gameState) return null;

    return gameState.gameStatus === 'Playing'
        ? <GameActive />
        : <GameLobby />;
}
