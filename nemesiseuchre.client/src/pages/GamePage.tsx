import { HubConnectionState } from '@microsoft/signalr';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { GameActive } from '@/components/GameActive';
import { GameLobby } from '@/components/GameLobby';
import { useEventAnimator } from '@/hooks/useEventAnimator';
import { useGameHub } from '@/signalr/useGameHub';
import type { PlayerGameState } from '@/types/game';

export function GamePage() {
    const { sessionName } = useParams<{ sessionName: string }>();
    const { connectionRef, connectionState } = useGameHub();
    const navigate = useNavigate();
    const [rawGameState, setRawGameState] = useState<PlayerGameState | null>(null);
    const { displayState, animationState } = useEventAnimator(rawGameState);

    useEffect(() => {
        if (connectionState !== HubConnectionState.Connected || !connectionRef.current || !sessionName) return;
        connectionRef.current.invoke<PlayerGameState>('JoinGameAsync', sessionName)
            .then(setRawGameState)
            .catch(err => console.error('JoinGame failed:', err));
    }, [connectionRef, connectionState, sessionName]);

    useEffect(() => {
        const conn = connectionRef.current;
        if (!conn) return;
        conn.on('ReceiveGameState', (updated: PlayerGameState) => setRawGameState(updated));
        conn.on('KickedFromSession', () => navigate('/'));
        return () => {
            conn.off('ReceiveGameState');
            conn.off('KickedFromSession');
        };
    }, [connectionRef, navigate]);

    if (!displayState) return null;

    return displayState.gameStatus === 'Playing'
        ? <GameActive gameState={displayState} animationState={animationState} connectionRef={connectionRef} />
        : <GameLobby gameState={displayState} connectionRef={connectionRef} />;
}
