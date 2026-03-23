import { Bot, Star } from 'lucide-react';
import type { RefObject } from 'react';
import type { HubConnection } from '@microsoft/signalr';
import { useAuth } from '@/auth/useAuth';
import type { AnimationState } from '@/hooks/useEventAnimator';
import type { Card, DealState, PlayerGameState, PlayerPosition, SeatOccupant, Team } from '@/types/game';
import { getPartnerPosition, toScreenPosition, type ScreenPosition } from '@/lib/boardRotation';
import { suitColor, suitSymbol } from '@/lib/cardUtils';
import { botDisplayName, seatDisplayName } from '@/lib/seatUtils';
import { getTeamLabel, getTeamForPosition } from '@/lib/teamUtils';
import { TRUMP_DECISION_LABELS, DEALER_TRUMP_DECISION_LABELS } from '@/lib/trumpUtils';
import { PlayerHand } from './PlayerHand';
import { PlayingCard } from './PlayingCard';
import { ScoreDisplay } from './ScoreDisplay';
import { UpCard } from './UpCard';

interface Props {
    gameState: PlayerGameState;
    animationState: AnimationState;
    connectionRef: RefObject<HubConnection | null>;
}


const SCREEN_SEAT_LAYOUT: Record<ScreenPosition, {
    container: string;
    labelPosition: 'above' | 'below' | 'left' | 'right';
    labelStyle?: string;
}> = {
    North: {
        container: 'absolute top-0 left-1/2 -translate-x-1/2 flex flex-col items-center gap-1 z-10',
        labelPosition: 'above',
    },
    South: {
        container: 'absolute bottom-0 left-1/2 -translate-x-1/2 flex flex-col items-center gap-1 z-10',
        labelPosition: 'below',
    },
    West: {
        container: 'absolute left-0 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1 z-10',
        labelPosition: 'left',
        labelStyle: '[writing-mode:vertical-rl] rotate-180',
    },
    East: {
        container: 'absolute right-0 top-1/2 -translate-y-1/2 flex flex-row items-center gap-1 z-10',
        labelPosition: 'right',
        labelStyle: '[writing-mode:vertical-rl]',
    },
};

const HAND_POSITION_CLASS: Record<ScreenPosition, string> = {
    North: 'absolute top-[148px] left-1/2 -translate-x-1/2 z-20',
    South: 'absolute bottom-[148px] left-1/2 -translate-x-1/2 z-20',
    West: 'absolute left-[148px] top-1/2 -translate-y-1/2 z-20',
    East: 'absolute right-[148px] top-1/2 -translate-y-1/2 z-20',
};

const PLAYED_CARD_POSITION: Record<ScreenPosition, string> = {
    North: 'absolute top-[35%] left-1/2 -translate-x-1/2 -translate-y-full z-30',
    South: 'absolute bottom-[35%] left-1/2 -translate-x-1/2 translate-y-full z-30',
    West: 'absolute left-[35%] top-1/2 -translate-y-1/2 -translate-x-full z-30',
    East: 'absolute right-[35%] top-1/2 -translate-y-1/2 translate-x-full z-30',
};

const SPEECH_BUBBLE_POSITION: Record<ScreenPosition, string> = {
    North: 'absolute top-[60px] left-1/2 -translate-x-1/2 z-40',
    South: 'absolute bottom-[60px] left-1/2 -translate-x-1/2 z-40',
    West: 'absolute left-[60px] top-1/2 -translate-y-1/2 z-40',
    East: 'absolute right-[60px] top-1/2 -translate-y-1/2 z-40',
};


type TrickEmphasis = 'normal' | 'won' | 'set' | 'march';

function getCallingTeam(deal: DealState): Team | null {
    if (!deal.callingPlayer) return null;
    return getTeamForPosition(deal.callingPlayer);
}

function getTrickCounts(deal: DealState): { team1: number; team2: number } {
    let team1 = 0;
    let team2 = 0;
    for (const trick of deal.completedTricks) {
        if (trick.winningTeam === 'Team1') team1++;
        else team2++;
    }
    return { team1, team2 };
}

function getTrickEmphasis(tricks: number, calling: Team | null, thisTeam: Team): TrickEmphasis {
    if (tricks === 5) return 'march';
    if (tricks >= 3 && calling !== null && calling !== thisTeam) return 'set';
    if (tricks >= 3) return 'won';
    return 'normal';
}

const EMPHASIS_STYLES: Record<TrickEmphasis, string> = {
    normal: 'text-muted-foreground',
    won: 'text-green-400',
    set: 'text-amber-400',
    march: 'text-yellow-300 font-black',
};

function TrickCount({ count, emphasis }: { count: number; emphasis: TrickEmphasis }) {
    const label = emphasis === 'march' ? 'March!' : emphasis === 'set' ? 'Euchred!' : '';
    return (
        <div className="flex flex-col items-center">
            <span className={`text-sm font-bold ${EMPHASIS_STYLES[emphasis]}`}>
                {count}/5 tricks
            </span>
            {label && (
                <span className={`text-[10px] font-semibold uppercase tracking-wide ${EMPHASIS_STYLES[emphasis]}`}>
                    {label}
                </span>
            )}
        </div>
    );
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

function ActiveSeatCard({
    occupant,
    isMe,
    isDealer,
    isDeciding,
    calledTrump,
    isGoingAlone,
}: {
    occupant: SeatOccupant | undefined;
    isMe: boolean;
    isDealer: boolean;
    isDeciding: boolean;
    calledTrump: boolean;
    isGoingAlone: boolean;
}) {
    const isBot = occupant !== undefined && occupant.gitHubLogin === null;
    const name = occupant ? (isBot ? botDisplayName(occupant) : occupant.gitHubLogin) : null;

    return (
        <div className={`relative flex flex-col items-center gap-1 rounded-xl border-2 p-2 w-28 text-sm shadow-md ${
            isDeciding
                ? 'ring-2 ring-amber-400 animate-pulse'
                : ''
        } ${
            isMe
                ? 'bg-background border-primary'
                : occupant
                ? 'bg-muted border-border'
                : 'bg-background/60 border-dashed border-muted-foreground/40'
        }`}>
            {isDealer && (
                <div
                    className="absolute -top-2 -right-2 w-5 h-5 rounded-full bg-yellow-400 text-black text-[10px] font-bold flex items-center justify-center shadow z-10"
                    title="Dealer"
                >
                    D
                </div>
            )}
            {calledTrump && (
                <div
                    className="absolute -top-2 -left-2 w-5 h-5 rounded-full bg-blue-500 text-white text-[10px] font-bold flex items-center justify-center shadow z-10"
                    title={isGoingAlone ? 'Called trump — going alone' : 'Called trump'}
                >
                    {isGoingAlone ? 'A' : 'T'}
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

export function GameActive({ gameState, animationState, connectionRef }: Props) {
    const { user } = useAuth();
    const myLogin = user?.login;

    const deal = gameState.currentDeal;
    const screen = (pos: PlayerPosition) => toScreenPosition(pos, gameState.myPosition);

    function handForPosition(position: PlayerPosition): { cards: Card[] | null; count: number } {
        if (!deal) return { cards: null, count: 0 };
        if (position === gameState.myPosition) return { cards: deal.myHand, count: deal.myHand.length };
        return { cards: null, count: deal.otherHandCounts[position] ?? 0 };
    }

    function seatCard(position: PlayerPosition) {
        const occupant = gameState.seats[position];
        const isMe = occupant?.gitHubLogin === myLogin;
        const isDealer = deal?.dealerPosition === position;
        const isDeciding = deal?.currentDeciderPosition === position;
        const calledTrump = deal?.callingPlayer === position;
        const isGoingAlone = calledTrump && (deal?.callingPlayerIsGoingAlone ?? false);
        return (
            <ActiveSeatCard
                occupant={occupant}
                isMe={isMe}
                isDealer={isDealer ?? false}
                isDeciding={isDeciding ?? false}
                calledTrump={calledTrump ?? false}
                isGoingAlone={isGoingAlone ?? false}
            />
        );
    }

    const isMyTrumpTurn = deal?.validTrumpDecisions !== null && deal?.validTrumpDecisions !== undefined;
    const isMyDiscardTurn = deal?.validDiscardCards !== null && deal?.validDiscardCards !== undefined;
    const isMyCardPlayTurn = deal?.validCardsToPlay !== null && deal?.validCardsToPlay !== undefined;
    const isDealer = deal?.dealerPosition === gameState.myPosition;
    const tricks = deal ? getTrickCounts(deal) : null;
    const calling = deal ? getCallingTeam(deal) : null;
    const showWaitingIndicator =
        deal?.currentDeciderPosition !== null &&
        deal?.currentDeciderPosition !== undefined &&
        deal.currentDeciderPosition !== gameState.myPosition &&
        !isMyTrumpTurn &&
        !isMyDiscardTurn &&
        !isMyCardPlayTurn;

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
                        {/* Score + trick counts — felt top corners */}
                        <div className="absolute top-3 left-4 flex flex-col items-center gap-1">
                            <ScoreDisplay teamName="Team 1" score={gameState.team1Score} />
                            {tricks && tricks.team1 + tricks.team2 > 0 && (
                                <TrickCount count={tricks.team1} emphasis={getTrickEmphasis(tricks.team1, calling, 'Team1')} />
                            )}
                        </div>
                        <div className="absolute top-3 right-4 flex flex-col items-center gap-1">
                            <ScoreDisplay teamName="Team 2" score={gameState.team2Score} />
                            {tricks && tricks.team1 + tricks.team2 > 0 && (
                                <TrickCount count={tricks.team2} emphasis={getTrickEmphasis(tricks.team2, calling, 'Team2')} />
                            )}
                        </div>
                        <img src="/nemesiseuchreLogo.svg" alt="NemesisEuchre" className="h-10 w-10 opacity-60" />
                        <span className="text-sm font-semibold tracking-tight opacity-60">
                            <span className="text-brand-blue">Nemesis</span>
                            <span className="text-brand-red">Euchre</span>
                        </span>

                        {deal?.trump && (
                            <div className="flex items-center gap-2 mt-1">
                                <span className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">Trump</span>
                                <span className={`text-3xl ${suitColor(deal.trump)}`}>{suitSymbol(deal.trump)}</span>
                            </div>
                        )}

                        {/* Waiting indicator */}
                        {showWaitingIndicator && deal?.currentDeciderPosition && (
                            <p className="text-sm text-muted-foreground mt-2">
                                Waiting for {seatDisplayName(deal.currentDeciderPosition, gameState.seats)} to decide...
                            </p>
                        )}
                    </div>

                    {(['North', 'East', 'South', 'West'] as PlayerPosition[]).map(gamePos => {
                        const sp = screen(gamePos);
                        const layout = SCREEN_SEAT_LAYOUT[sp];
                        const team = getTeamLabel(gamePos);
                        const label = (
                            <span className={`text-[10px] text-muted-foreground/60 uppercase tracking-widest font-medium ${layout.labelStyle ?? ''}`}>
                                {team}
                            </span>
                        );
                        return (
                            <div key={gamePos} className={layout.container}>
                                {(layout.labelPosition === 'above' || layout.labelPosition === 'left') && label}
                                {seatCard(gamePos)}
                                {(layout.labelPosition === 'below' || layout.labelPosition === 'right') && label}
                            </div>
                        );
                    })}

                    {(['North', 'East', 'South', 'West'] as PlayerPosition[]).map(gamePos => {
                        const sp = screen(gamePos);
                        const isMyPos = gamePos === gameState.myPosition;

                        const goingAlonePartner = deal?.callingPlayer && deal.callingPlayerIsGoingAlone
                            ? getPartnerPosition(deal.callingPlayer)
                            : null;
                        if (gamePos === goingAlonePartner) return null;

                        const canInteract = isMyPos && (isMyDiscardTurn || isMyCardPlayTurn);
                        return (
                            <div key={gamePos} className={HAND_POSITION_CLASS[sp]}>
                                <PlayerHand
                                    {...handForPosition(gamePos)}
                                    position={sp}
                                    onCardClick={canInteract
                                        ? (card) => {
                                            if (isMyDiscardTurn) connectionRef.current?.invoke('MakeDealerDiscardAsync', card);
                                            else connectionRef.current?.invoke('PlayCardAsync', card);
                                        }
                                        : undefined}
                                    highlightCards={canInteract}
                                    validCards={isMyPos && isMyCardPlayTurn ? deal!.validCardsToPlay! : undefined}
                                />
                            </div>
                        );
                    })}

                    {(deal?.dealStatus === 'SelectingTrumpPhase1' || deal?.dealStatus === 'SelectingTrumpPhase2'
                        || animationState.upCardAnimating) && deal?.dealerPosition && (
                        <UpCard
                            card={animationState.upCardAnimating === 'flipping' ? null : (deal.dealStatus === 'SelectingTrumpPhase1' ? deal.upCard : null)}
                            dealerPosition={screen(deal.dealerPosition)}
                            animating={animationState.upCardAnimating}
                        />
                    )}

                    {animationState.activeBubble && (
                        <div className={SPEECH_BUBBLE_POSITION[screen(animationState.activeBubble.position)]}>
                            <div className="bg-background border border-border rounded-lg px-3 py-1.5 text-sm font-semibold shadow-lg animate-in fade-in zoom-in-95 duration-200">
                                {animationState.activeBubble.text}
                            </div>
                        </div>
                    )}

                    {(animationState.overrideTrickCards ?? deal?.currentTrickCards)?.map(pc => (
                        <div key={`${pc.card.suit}-${pc.card.rank}`} className={PLAYED_CARD_POSITION[screen(pc.playerPosition)]}>
                            <PlayingCard card={pc.card} />
                        </div>
                    ))}
                </div>

                {/* Trump decision panel — below table */}
                {isMyTrumpTurn && (
                    <div className="bg-background/90 border border-border rounded-xl px-6 py-4 shadow-lg flex flex-col items-center gap-3">
                        <p className="text-sm font-semibold text-amber-500 uppercase tracking-wide">Your turn to decide</p>
                        <div className="flex flex-wrap justify-center gap-2 max-w-[480px]">
                            {deal!.validTrumpDecisions!.map(d => (
                                <button
                                    key={d}
                                    className="px-4 py-2 text-sm rounded-md border border-border bg-background hover:bg-accent transition-colors cursor-pointer"
                                    onClick={() => connectionRef.current?.invoke('MakeTrumpDecisionAsync', d)}
                                >
                                    {(isDealer && DEALER_TRUMP_DECISION_LABELS[d]) || TRUMP_DECISION_LABELS[d]}
                                </button>
                            ))}
                        </div>
                    </div>
                )}

                {/* Dealer discard panel — below table */}
                {isMyDiscardTurn && (
                    <div className="bg-background/90 border border-border rounded-xl px-6 py-4 shadow-lg flex flex-col items-center gap-2">
                        <p className="text-sm font-semibold text-amber-500 uppercase tracking-wide">Choose a card to discard</p>
                        <p className="text-sm text-muted-foreground">Click a card in your hand above</p>
                    </div>
                )}

                {/* Deal result display */}
                {animationState.dealResultMessage && (
                    <div className="bg-background/90 border border-border rounded-xl px-6 py-4 shadow-lg text-center animate-in fade-in zoom-in-95 duration-300">
                        <p className="text-lg font-semibold">{animationState.dealResultMessage}</p>
                    </div>
                )}
            </div>
        </div>
    );
}
