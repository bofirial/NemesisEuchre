import { HubConnectionState } from '@microsoft/signalr';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { GameActive } from '@/components/GameActive';
import { GameLobby } from '@/components/GameLobby';
import { useGameHub } from '@/signalr/useGameHub';
import type { PlayerGameState } from '@/types/game';

export function GamePage() {
    const { sessionName } = useParams<{ sessionName: string }>();
    const { connectionRef, connectionState } = useGameHub();
    const navigate = useNavigate();
    const [gameState, setGameState] = useState<PlayerGameState | null>(null);

    useEffect(() => {
        if (connectionState !== HubConnectionState.Connected || !connectionRef.current || !sessionName) return;
        connectionRef.current.invoke<PlayerGameState>('JoinGameAsync', sessionName)
            .then(setGameState)
            .catch(err => console.error('JoinGame failed:', err));
    }, [connectionRef, connectionState, sessionName]);

    useEffect(() => {
        const conn = connectionRef.current;
        if (!conn) return;
        conn.on('ReceiveGameState', (updated: PlayerGameState) => setGameState(updated));
        conn.on('KickedFromSession', () => navigate('/'));
        return () => {
            conn.off('ReceiveGameState');
            conn.off('KickedFromSession');
        };
    }, [connectionRef, navigate]);

    if (!gameState) return null;

    return gameState.gameStatus === 'Playing'
        ? <GameActive gameState={gameState} connectionRef={connectionRef} />
        : <GameLobby gameState={gameState} connectionRef={connectionRef} />;
}
