import { Crown, Star, UserX } from 'lucide-react';
import type { RefObject } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import { useAuth } from '@/auth/useAuth';
import type { PlayerGameState } from '@/types/game';

interface Props {
    gameState: PlayerGameState;
    connectionRef: RefObject<HubConnection | null>;
}

export function GameLobby({ gameState, connectionRef }: Props) {
    const { user } = useAuth();
    const amLeader = gameState.connectedUsers.some(
        u => u.gitHubLogin === user?.login && u.isSessionLeader
    );

    return (
        <div className="flex flex-col items-center gap-4">
            <h2 className="text-2xl font-semibold">Game Lobby — {gameState.sessionName}</h2>
            <ul className="flex flex-col gap-1">
                {gameState.connectedUsers.map(u => (
                    <li key={u.gitHubLogin} className="flex items-center gap-2 text-sm">
                        {u.isSessionLeader && <Star className="size-4 fill-yellow-400 text-yellow-400" />}
                        <span className="flex-1">{u.gitHubLogin}</span>
                        {amLeader && u.gitHubLogin !== user?.login && (
                            <>
                                {!u.isSessionLeader && (
                                    <button
                                        title="Promote to session leader"
                                        onClick={() => connectionRef.current?.invoke('PromoteToLeaderAsync', u.gitHubLogin)}
                                        className="text-muted-foreground hover:text-foreground transition-colors"
                                    >
                                        <Crown className="size-4" />
                                    </button>
                                )}
                                <button
                                    title="Remove from session"
                                    onClick={() => connectionRef.current?.invoke('RemoveUserFromSessionAsync', u.gitHubLogin)}
                                    className="text-muted-foreground hover:text-destructive transition-colors"
                                >
                                    <UserX className="size-4" />
                                </button>
                            </>
                        )}
                    </li>
                ))}
            </ul>
        </div>
    );
}
