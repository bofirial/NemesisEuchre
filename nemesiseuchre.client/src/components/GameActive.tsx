import { Bot, Star } from 'lucide-react';
import type { RefObject } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import { useAuth } from '@/auth/useAuth';
import type { Card, PlayerGameState, PlayerPosition, SeatOccupant } from '@/types/game';
import { PlayerHand } from './PlayerHand';
import { ScoreDisplay } from './ScoreDisplay';
import { UpCard } from './UpCard';

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

function ActiveSeatCard({ occupant, isMe, isDealer }: { occupant: SeatOccupant | undefined; isMe: boolean; isDealer: boolean }) {
    const isBot = occupant !== undefined && occupant.gitHubLogin === null;
    const name = occupant ? (isBot ? botDisplayName(occupant) : occupant.gitHubLogin) : null;

    return (
        <div className={`relative flex flex-col items-center gap-1 rounded-xl border-2 p-2 w-28 text-sm shadow-md ${
            isMe
                ? 'bg-background border-primary'
                : occupant
                ? 'bg-muted border-border'
                : 'bg-background/60 border-dashed border-muted-foreground/40'
        }`}>
            {isDealer && (
                <div className="absolute -top-2 -right-2 w-5 h-5 rounded-full bg-yellow-400 text-black text-[10px] font-bold flex items-center justify-center shadow z-10">
                    D
                </div>
            )}
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

    const deal = gameState.currentDeal;

    function handForPosition(position: PlayerPosition): { cards: Card[] | null; count: number } {
        if (!deal) return { cards: null, count: 0 };
        if (position === gameState.myPosition) return { cards: deal.myHand, count: deal.myHand.length };
        return { cards: null, count: deal.otherHandCounts[position] ?? 0 };
    }

    function seatCard(position: PlayerPosition) {
        const occupant = gameState.seats[position];
        const isMe = occupant?.gitHubLogin === myLogin;
        const isDealer = deal?.dealerPosition === position;
        return <ActiveSeatCard occupant={occupant} isMe={isMe} isDealer={isDealer ?? false} />;
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
                <div className="relative w-[900px] h-[680px]">

                    {/* Felt table */}
                    <div className="absolute inset-[140px] rounded-3xl bg-muted/40 border-4 border-border shadow-inner flex flex-col items-center justify-center gap-2 select-none">
                        {/* Score — felt top corners */}
                        <div className="absolute top-3 left-4">
                            <ScoreDisplay teamName="Team 1" score={gameState.team1Score} />
                        </div>
                        <div className="absolute top-3 right-4">
                            <ScoreDisplay teamName="Team 2" score={gameState.team2Score} />
                        </div>
                        <img src="/nemesiseuchreLogo.svg" alt="NemesisEuchre" className="h-10 w-10 opacity-60" />
                        <span className="text-sm font-semibold tracking-tight opacity-60">
                            <span className="text-brand-blue">Nemesis</span>
                            <span className="text-brand-red">Euchre</span>
                        </span>
                    </div>

                    {/* North seat — TEAM 2 */}
                    <div className="absolute top-0 left-1/2 -translate-x-1/2 flex flex-col items-center gap-1 z-10">
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium">Team 2</span>
                        {seatCard('North')}
                    </div>

                    {/* South seat — TEAM 2 */}
                    <div className="absolute bottom-0 left-1/2 -translate-x-1/2 flex flex-col items-center gap-1 z-10">
                        {seatCard('South')}
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium">Team 2</span>
                    </div>

                    {/* West seat — TEAM 1 */}
                    <div className="absolute left-0 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1 z-10">
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium [writing-mode:vertical-rl] rotate-180">Team 1</span>
                        {seatCard('West')}
                    </div>

                    {/* East seat — TEAM 1 */}
                    <div className="absolute right-0 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1 z-10">
                        {seatCard('East')}
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium [writing-mode:vertical-rl]">Team 1</span>
                    </div>

                    {/* Hands — positioned just inside the felt border (inset 140px → hands at 148px) */}
                    <div className="absolute top-[148px] left-1/2 -translate-x-1/2 z-20">
                        <PlayerHand {...handForPosition('North')} position="North" />
                    </div>
                    <div className="absolute bottom-[148px] left-1/2 -translate-x-1/2 z-20">
                        <PlayerHand {...handForPosition('South')} position="South" />
                    </div>
                    <div className="absolute left-[148px] top-1/2 -translate-y-1/2 z-20">
                        <PlayerHand {...handForPosition('West')} position="West" />
                    </div>
                    <div className="absolute right-[148px] top-1/2 -translate-y-1/2 z-20">
                        <PlayerHand {...handForPosition('East')} position="East" />
                    </div>

                    {(deal?.dealStatus === 'SelectingTrumpPhase1' || deal?.dealStatus === 'SelectingTrumpPhase2') && deal.dealerPosition && (
                        <UpCard
                            card={deal.dealStatus === 'SelectingTrumpPhase1' ? deal.upCard : null}
                            dealerPosition={deal.dealerPosition}
                        />
                    )}
                </div>
            </div>
        </div>
    );
}
