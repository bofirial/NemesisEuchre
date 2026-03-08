import { Bot, Crown, Star, UserX } from 'lucide-react';
import { useState, useEffect } from 'react';
import type { RefObject } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import { useAuth } from '@/auth/useAuth';
import { authFetch } from '@/api/fetchUtils';
import type { ActorType, PlayerGameState, PlayerPosition, SeatOccupant } from '@/types/game';

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

function Avatar({ occupant }: { occupant: SeatOccupant | undefined }) {
    if (!occupant) {
        return (
            <div className="w-12 h-12 rounded-full border-2 border-dashed border-muted-foreground/40 flex items-center justify-center">
                <span className="text-muted-foreground/40 text-xl">?</span>
            </div>
        );
    }
    if (occupant.gitHubLogin) {
        return (
            <img
                src={`https://github.com/${occupant.gitHubLogin}.png?size=96`}
                alt={occupant.gitHubLogin}
                className="w-12 h-12 rounded-full border-2 border-border object-cover"
            />
        );
    }
    return (
        <div className="w-12 h-12 rounded-full border-2 border-border bg-muted flex items-center justify-center">
            <Bot className="size-6 text-muted-foreground" />
        </div>
    );
}

interface SeatCardProps {
    position: PlayerPosition;
    occupant: SeatOccupant | undefined;
    isMe: boolean;
    amSeated: boolean;
    amLeader: boolean;
    addBotSeat: PlayerPosition | null;
    azureBots: string[] | null;
    onClaim: () => void;
    onRemoveBot: () => void;
    onToggleAddBot: () => void;
    onAddBot: (_type: ActorType, _name?: string) => void;
    onCancelAddBot: () => void;
    onVacate: () => void;
}

function SeatCard({
    position, occupant, isMe, amSeated, amLeader,
    addBotSeat, azureBots,
    onClaim, onRemoveBot, onToggleAddBot, onAddBot, onCancelAddBot, onVacate,
}: SeatCardProps) {
    const isBot = occupant !== undefined && occupant.gitHubLogin === null;
    const showBotPicker = addBotSeat === position;

    return (
        <div
            className={`relative flex flex-col items-center gap-1.5 rounded-xl border-2 p-3 w-36 text-sm shadow-md ${
                isMe
                    ? 'bg-background border-primary'
                    : occupant
                    ? 'bg-muted border-border'
                    : 'bg-background/60 border-dashed border-muted-foreground/40'
            }`}
        >
            <Avatar occupant={occupant} />

            {occupant ? (
                <>
                    <span className={`font-semibold text-center break-words leading-tight ${isMe ? 'text-primary' : ''}`}>
                        {isBot ? botDisplayName(occupant) : occupant.gitHubLogin}
                    </span>
                    {isMe && (
                        <button
                            onClick={onVacate}
                            className="cursor-pointer mt-1 px-2 py-0.5 rounded text-xs font-semibold bg-destructive/10 text-destructive hover:bg-destructive hover:text-destructive-foreground transition-colors border border-destructive/30"
                        >
                            LEAVE SEAT
                        </button>
                    )}
                    {amLeader && isBot && (
                        <button
                            title="Remove bot"
                            onClick={onRemoveBot}
                            className="cursor-pointer text-xs text-muted-foreground hover:text-destructive transition-colors"
                        >
                            Remove
                        </button>
                    )}
                </>
            ) : (
                <>
                    <button
                        onClick={onClaim}
                        className="cursor-pointer px-2 py-0.5 rounded text-xs font-medium bg-primary/10 text-primary hover:bg-primary hover:text-primary-foreground transition-colors border border-primary/30"
                    >
                        {amSeated ? 'Move here' : 'Join'}
                    </button>
                    {amLeader && (
                        <button
                            onClick={onToggleAddBot}
                            className="cursor-pointer text-xs text-muted-foreground hover:text-foreground transition-colors underline"
                        >
                            Add Bot
                        </button>
                    )}
                </>
            )}

            {showBotPicker && (
                <div className="absolute top-full mt-1 z-20 bg-popover border border-border rounded-lg shadow-lg p-2 flex flex-col items-center gap-0.5 w-36">
                    {(['Chaos', 'Beta', 'Chad'] as ActorType[]).map(type => (
                        <button
                            key={type}
                            onClick={() => onAddBot(type)}
                            className="cursor-pointer w-full text-xs text-center hover:bg-accent rounded px-1 py-0.5"
                        >
                            {type}Bot
                        </button>
                    ))}
                    {(azureBots ?? []).map(name => (
                        <button
                            key={name}
                            onClick={() => onAddBot('Model', name)}
                            className="cursor-pointer w-full text-xs text-center hover:bg-accent rounded px-1 py-0.5"
                        >
                            {name}
                        </button>
                    ))}
                    <button
                        onClick={onCancelAddBot}
                        className="cursor-pointer mt-1 text-xs text-muted-foreground hover:text-foreground underline"
                    >
                        Cancel
                    </button>
                </div>
            )}
        </div>
    );
}

export function GameLobby({ gameState, connectionRef }: Props) {
    const { user } = useAuth();
    const myLogin = user?.login;
    const amLeader = gameState.connectedUsers.some(
        u => u.gitHubLogin === myLogin && u.isSessionLeader
    );
    const amSeated = Object.values(gameState.seats).some(s => s?.gitHubLogin === myLogin);
    const seatedCount = Object.values(gameState.seats).filter(Boolean).length;
    const allSeated = seatedCount === 4;

    const [addBotSeat, setAddBotSeat] = useState<PlayerPosition | null>(null);
    const [azureBots, setAzureBots] = useState<string[] | null>(null);

    useEffect(() => {
        if (addBotSeat === null) return;
        if (azureBots !== null) return;

        authFetch('/api/bots')
            .then(r => r.ok ? r.json() : { azureBots: [] })
            .then((data: { builtinBots?: string[]; azureBots?: string[] }) => {
                setAzureBots(data.azureBots ?? []);
            })
            .catch(() => setAzureBots([]));
    }, [addBotSeat, azureBots]);

    function claimSeat(position: PlayerPosition) {
        connectionRef.current?.invoke('ClaimSeatAsync', position);
    }

    function addBot(position: PlayerPosition, actorType: ActorType, modelName?: string) {
        connectionRef.current?.invoke('AddBotToSeatAsync', position, actorType, modelName ?? null);
        setAddBotSeat(null);
    }

    function removeBot(position: PlayerPosition) {
        connectionRef.current?.invoke('RemoveBotFromSeatAsync', position);
    }

    function makeSeatProps(position: PlayerPosition): Omit<SeatCardProps, 'position'> {
        const occupant = gameState.seats[position];
        const isMe = occupant?.gitHubLogin === myLogin;
        const isBot = occupant !== undefined && occupant.gitHubLogin === null;
        return {
            occupant,
            isMe: isMe,
            amSeated,
            amLeader,
            addBotSeat,
            azureBots,
            onClaim: () => claimSeat(position),
            onRemoveBot: () => { if (isBot) removeBot(position); },
            onToggleAddBot: () => setAddBotSeat(addBotSeat === position ? null : position),
            onAddBot: (type, name) => addBot(position, type, name),
            onCancelAddBot: () => setAddBotSeat(null),
            onVacate: () => connectionRef.current?.invoke('VacateSeatAsync'),
        };
    }

    return (
        <div className="relative w-full flex justify-center">

            {/* Left sidebar — connected users, floats in the left gutter */}
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
                            {amLeader && u.gitHubLogin !== myLogin && (
                                <>
                                    {!u.isSessionLeader && (
                                        <button
                                            title="Promote to session leader"
                                            onClick={() => connectionRef.current?.invoke('PromoteToLeaderAsync', u.gitHubLogin)}
                                            className="text-muted-foreground hover:text-foreground transition-colors"
                                        >
                                            <Crown className="size-3.5" />
                                        </button>
                                    )}
                                    <button
                                        title="Remove from session"
                                        onClick={() => connectionRef.current?.invoke('RemoveUserFromSessionAsync', u.gitHubLogin)}
                                        className="text-muted-foreground hover:text-destructive transition-colors"
                                    >
                                        <UserX className="size-3.5" />
                                    </button>
                                </>
                            )}
                        </li>
                    ))}
                </ul>
            </div>

            {/* Table + title + start game — centered */}
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

                    {/* North seat — TEAM 2 */}
                    <div className={`absolute top-0 left-1/2 -translate-x-1/2 flex flex-col items-center gap-1 ${addBotSeat === 'North' ? 'z-30' : 'z-10'}`}>
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium">Team 2</span>
                        <SeatCard position="North" {...makeSeatProps('North')} />
                    </div>

                    {/* South seat — TEAM 2 */}
                    <div className={`absolute bottom-0 left-1/2 -translate-x-1/2 flex flex-col items-center gap-1 ${addBotSeat === 'South' ? 'z-30' : 'z-10'}`}>
                        <SeatCard position="South" {...makeSeatProps('South')} />
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium">Team 2</span>
                    </div>

                    {/* West seat — TEAM 1 */}
                    <div className={`absolute left-0 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1 ${addBotSeat === 'West' ? 'z-30' : 'z-10'}`}>
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium [writing-mode:vertical-rl] rotate-180">Team 1</span>
                        <SeatCard position="West" {...makeSeatProps('West')} />
                    </div>

                    {/* East seat — TEAM 1 */}
                    <div className={`absolute right-0 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1 ${addBotSeat === 'East' ? 'z-30' : 'z-10'}`}>
                        <SeatCard position="East" {...makeSeatProps('East')} />
                        <span className="text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium [writing-mode:vertical-rl]">Team 1</span>
                    </div>
                </div>

                {/* Start Game */}
                {amLeader && (
                    <div className="flex flex-col items-center gap-1">
                        <button
                            disabled={!allSeated}
                            onClick={() => connectionRef.current?.invoke('StartGameAsync')}
                            className={`px-8 py-2.5 rounded-lg font-bold text-base transition-colors ${
                                allSeated
                                    ? 'cursor-pointer bg-green-600 hover:bg-green-500 text-white shadow-md'
                                    : 'cursor-not-allowed bg-muted text-muted-foreground'
                            }`}
                        >
                            START GAME
                        </button>
                        <span className="text-xs text-muted-foreground">
                            {seatedCount}/4 players{allSeated ? ' (Ready to Start)' : ''}
                        </span>
                    </div>
                )}
            </div>
        </div>
    );
}
