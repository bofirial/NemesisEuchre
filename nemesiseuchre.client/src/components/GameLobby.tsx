import { Crown, Star, UserX } from 'lucide-react';
import type { RefObject } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import { useAuth } from '@/auth/useAuth';
import type { PlayerGameState, PlayerPosition } from '@/types/game';

interface Props {
    gameState: PlayerGameState;
    connectionRef: RefObject<HubConnection | null>;
}

const SEAT_POSITIONS: { position: PlayerPosition; gridClass: string }[] = [
    { position: 'North', gridClass: 'col-start-2 row-start-1' },
    { position: 'West',  gridClass: 'col-start-1 row-start-2' },
    { position: 'East',  gridClass: 'col-start-3 row-start-2' },
    { position: 'South', gridClass: 'col-start-2 row-start-3' },
];

export function GameLobby({ gameState, connectionRef }: Props) {
    const { user } = useAuth();
    const myLogin = user?.login;
    const amLeader = gameState.connectedUsers.some(
        u => u.gitHubLogin === myLogin && u.isSessionLeader
    );
    const amSeated = Object.values(gameState.seats).some(s => s?.gitHubLogin === myLogin);

    function claimSeat(position: PlayerPosition) {
        connectionRef.current?.invoke('ClaimSeatAsync', position);
    }

    return (
        <div className="flex flex-col items-center gap-6">
            <h2 className="text-2xl font-semibold">Game Lobby — {gameState.sessionName}</h2>

            <div className="grid grid-cols-3 grid-rows-3 gap-3 w-72">
                {SEAT_POSITIONS.map(({ position, gridClass }) => {
                    const occupant = gameState.seats[position];
                    const isMe = occupant?.gitHubLogin === myLogin;

                    return (
                        <div
                            key={position}
                            className={`${gridClass} flex flex-col items-center justify-center rounded-lg border p-3 min-h-16 text-sm ${
                                occupant ? 'bg-muted' : 'border-dashed'
                            }`}
                        >
                            {occupant ? (
                                <span className={`font-medium text-center break-all ${isMe ? 'text-primary' : ''}`}>
                                    {occupant.gitHubLogin}
                                </span>
                            ) : (
                                <button
                                    onClick={() => claimSeat(position)}
                                    className="cursor-pointer text-xs text-muted-foreground hover:text-foreground transition-colors underline"
                                >
                                    {amSeated ? 'Move here' : 'Join'}
                                </button>
                            )}
                        </div>
                    );
                })}

                <div className="col-start-2 row-start-2 flex items-center justify-center rounded-lg bg-muted/30 min-h-16">
                    <span className="text-xs text-muted-foreground">Table</span>
                </div>
            </div>

            <ul className="flex flex-col gap-1 w-full max-w-xs">
                {gameState.connectedUsers.map(u => (
                    <li key={u.gitHubLogin} className="flex items-center gap-2 text-sm">
                        {u.isSessionLeader && <Star className="size-4 fill-yellow-400 text-yellow-400" />}
                        <span className="flex-1">{u.gitHubLogin}</span>
                        {amLeader && u.gitHubLogin !== myLogin && (
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

            {amSeated && (
                <button
                    onClick={() => connectionRef.current?.invoke('VacateSeatAsync')}
                    className="cursor-pointer text-sm text-muted-foreground hover:text-foreground transition-colors underline"
                >
                    Spectate
                </button>
            )}
        </div>
    );
}
