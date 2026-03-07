import { Bot, Star } from 'lucide-react';
import type { RefObject } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import { useAuth } from '@/auth/useAuth';
import type { PlayerGameState, PlayerPosition, SeatOccupant } from '@/types/game';

interface Props {
    gameState: PlayerGameState;
    connectionRef: RefObject<HubConnection | null>;
}

function botDisplayName(seat: SeatOccupant): string {
    if (seat.botModelName) return seat.botModelName;
    if (seat.botActorType === 'Chaos') return 'ChaosBot';
    if (seat.botActorType === 'Beta') return 'BetaBot';
    if (seat.botActorType === 'Chad') return 'ChadBot';
    return 'Bot';
}

function SmallAvatar({ occupant }: { occupant: SeatOccupant | undefined }) {
    if (!occupant) {
        return (
            <div className="w-8 h-8 rounded-full border-2 border-dashed border-muted-foreground/40 flex items-center justify-center">
                <span className="text-muted-foreground/40 text-sm">?</span>
            </div>
        );
    }
    if (occupant.gitHubLogin) {
        return (
            <img
                src={`https://github.com/${occupant.gitHubLogin}.png?size=64`}
                alt={occupant.gitHubLogin}
                className="w-8 h-8 rounded-full border-2 border-border object-cover"
            />
        );
    }
    return (
        <div className="w-8 h-8 rounded-full border-2 border-border bg-muted flex items-center justify-center">
            <Bot className="size-4 text-muted-foreground" />
        </div>
    );
}

function ActiveSeatCard({ occupant, isMe }: { occupant: SeatOccupant | undefined; isMe: boolean }) {
    const isBot = occupant !== undefined && occupant.gitHubLogin === null;
    const name = occupant ? (isBot ? botDisplayName(occupant) : occupant.gitHubLogin) : null;

    return (
        <div className={`flex flex-col items-center gap-1 rounded-xl border-2 p-2 w-28 text-sm shadow-md ${
            isMe
                ? 'bg-background border-primary'
                : occupant
                ? 'bg-muted border-border'
                : 'bg-background/60 border-dashed border-muted-foreground/40'
        }`}>
            <SmallAvatar occupant={occupant} />
            {name && (
                <span className={`font-semibold text-center break-words leading-tight text-xs ${isMe ? 'text-primary' : ''}`}>
                    {name}
                </span>
            )}
        </div>
    );
}

export function GameActive({ gameState }: Props) {
    const { user } = useAuth();
    const myLogin = user?.login;

    function seatCard(position: PlayerPosition) {
        const occupant = gameState.seats[position];
        const isMe = occupant?.gitHubLogin === myLogin;
        return <ActiveSeatCard occupant={occupant} isMe={isMe} />;
    }

    return (
        <div className="relative w-full flex justify-center">

            {/* Left sidebar — connected users */}
            <div className="absolute left-0 top-0 flex flex-col gap-3 w-44 pt-2">
                <h3 className="text-xs font-semibold uppercase tracking-widest text-muted-foreground">User List</h3>
                <ul className="flex flex-col gap-2">
                    {gameState.connectedUsers.map(u => (
                        <li key={u.gitHubLogin} className="flex items-center gap-2 text-sm">
                            <div className="relative shrink-0">
                                <img
                                    src={`https://github.com/${u.gitHubLogin}.png?size=64`}
                                    alt={u.gitHubLogin}
                                    className="w-8 h-8 rounded-full border border-border object-cover"
                                />
                                {u.isSessionLeader && (
                                    <Star className="absolute -top-1 -right-1 size-3 fill-yellow-400 text-yellow-400" />
                                )}
                            </div>
                            <span className="flex-1 truncate">{u.gitHubLogin}</span>
                        </li>
                    ))}
                </ul>
            </div>

            {/* Table + title — centered */}
            <div className="flex flex-col items-center gap-4">
                <h2 className="text-2xl font-semibold">{gameState.sessionName}</h2>

                {/* Table area */}
                <div className="relative w-[640px] h-[480px]">

                    {/* Felt table */}
                    <div className="absolute inset-[108px] rounded-3xl bg-muted/40 border-4 border-border shadow-inner flex flex-col items-center justify-center gap-2 select-none">
                        <img src="/nemesiseuchreLogo.svg" alt="NemesisEuchre" className="h-10 w-10 opacity-60" />
                        <span className="text-sm font-semibold tracking-tight opacity-60">
                            <span className="text-brand-blue">Nemesis</span>
                            <span className="text-brand-red">Euchre</span>
                        </span>
                    </div>

                    {/* North seat — TEAM B */}
                    <div className="absolute top-0 left-1/2 -translate-x-1/2 flex flex-col items-center gap-1 z-10">
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium">Team B</span>
                        {seatCard('North')}
                    </div>

                    {/* South seat — TEAM B */}
                    <div className="absolute bottom-0 left-1/2 -translate-x-1/2 flex flex-col items-center gap-1 z-10">
                        {seatCard('South')}
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium">Team B</span>
                    </div>

                    {/* West seat — TEAM A */}
                    <div className="absolute left-0 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1 z-10">
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium [writing-mode:vertical-rl] rotate-180">Team A</span>
                        {seatCard('West')}
                    </div>

                    {/* East seat — TEAM A */}
                    <div className="absolute right-0 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1 z-10">
                        {seatCard('East')}
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium [writing-mode:vertical-rl]">Team A</span>
                    </div>
                </div>
            </div>
        </div>
    );
}
