import { useCallback, useEffect, useRef, useState } from 'react';
import type {
    Card,
    DealState,
    PlayedCard,
    PlayerGameEvent,
    PlayerGameState,
    PlayerPosition,
} from '@/types/game';
import { ANIMATION_DURATIONS } from '@/lib/animationTimings';
import { buildDealResultMessage } from '@/lib/dealResultUtils';
import { TRUMP_BUBBLE_LABELS } from '@/lib/trumpUtils';

export interface AnimationState {
    activeBubble: { position: PlayerPosition; text: string } | null;
    upCardAnimating: 'flipping' | 'pickup' | null;
    dealResultMessage: string | null;
    overrideTrickCards: PlayedCard[] | null;
}

const EMPTY_ANIMATION: AnimationState = {
    activeBubble: null,
    upCardAnimating: null,
    dealResultMessage: null,
    overrideTrickCards: null,
};

interface AnimationStep {
    animation: AnimationState;
    applyState?: PlayerGameState;
    eventIndex: number;
    durationMs: number;
}

function withDeal(state: PlayerGameState, deal: DealState): PlayerGameState {
    return { ...state, currentDeal: deal };
}

function removeCardFromHand(hand: Card[], card: Card): Card[] {
    const idx = hand.findIndex(c => c.suit === card.suit && c.rank === card.rank);
    if (idx === -1) return hand;
    return [...hand.slice(0, idx), ...hand.slice(idx + 1)];
}

function adjustOtherHandCount(
    counts: Partial<Record<PlayerPosition, number>>,
    position: PlayerPosition,
): Partial<Record<PlayerPosition, number>> {
    const current = counts[position];
    if (current === undefined || current <= 0) return counts;
    return { ...counts, [position]: current - 1 };
}

function buildSteps(
    events: PlayerGameEvent[],
    startAfter: number,
    rawState: PlayerGameState,
): AnimationStep[] {
    const steps: AnimationStep[] = [];
    const trickCards: PlayedCard[] = [];
    let currentDeal = rawState.currentDeal;
    const myPosition = rawState.myPosition;

    for (const event of events) {
        if (event.eventIndex <= startAfter) {
            if (event.eventType === 'CardPlayed') {
                trickCards.push({ card: event.card, playerPosition: event.position });
            } else if (event.eventType === 'TrickCompleted' || event.eventType === 'NewDealStarted') {
                trickCards.length = 0;
            }
            continue;
        }

        switch (event.eventType) {
            case 'NewDealStarted': {
                currentDeal = currentDeal ? {
                    ...currentDeal,
                    dealStatus: 'SelectingTrumpPhase1',
                    dealerPosition: event.dealerPosition,
                    upCard: event.upCard,
                    trump: null,
                    callingPlayer: null,
                    callingPlayerIsGoingAlone: false,
                    myHand: event.myHand,
                    otherHandCounts: {
                        ...Object.fromEntries(
                            (['North', 'East', 'South', 'West'] as PlayerPosition[])
                                .filter(p => p !== myPosition)
                                .map(p => [p, 5]),
                        ),
                    },
                    completedTricks: [],
                    currentTrickCards: [],
                    currentDeciderPosition: null,
                    validTrumpDecisions: null,
                    validDiscardCards: null,
                    validCardsToPlay: null,
                    trumpDecisions: [],
                    dealResult: null,
                    winningTeam: null,
                } : currentDeal;
                steps.push({
                    animation: EMPTY_ANIMATION,
                    applyState: currentDeal ? withDeal(rawState, currentDeal) : rawState,
                    eventIndex: event.eventIndex,
                    durationMs: 0,
                });
                break;
            }
            case 'TrumpDecisionMade':
                steps.push({
                    animation: {
                        ...EMPTY_ANIMATION,
                        activeBubble: { position: event.position, text: TRUMP_BUBBLE_LABELS[event.decision] },
                    },
                    eventIndex: event.eventIndex,
                    durationMs: ANIMATION_DURATIONS.TRUMP_DECISION,
                });
                break;
            case 'UpCardFlipped':
                steps.push({
                    animation: { ...EMPTY_ANIMATION, upCardAnimating: 'flipping' },
                    eventIndex: event.eventIndex,
                    durationMs: ANIMATION_DURATIONS.UP_CARD_FLIP,
                });
                break;
            case 'UpCardPickedUp':
                if (currentDeal && myPosition === event.dealerPosition) {
                    currentDeal = {
                        ...currentDeal,
                        myHand: currentDeal.upCard
                            ? [...currentDeal.myHand, currentDeal.upCard]
                            : currentDeal.myHand,
                    };
                } else if (currentDeal) {
                    currentDeal = {
                        ...currentDeal,
                        otherHandCounts: {
                            ...currentDeal.otherHandCounts,
                            [event.dealerPosition]: (currentDeal.otherHandCounts[event.dealerPosition] ?? 5) + 1,
                        },
                    };
                }
                steps.push({
                    animation: { ...EMPTY_ANIMATION, upCardAnimating: 'pickup' },
                    applyState: currentDeal ? withDeal(rawState, currentDeal) : rawState,
                    eventIndex: event.eventIndex,
                    durationMs: ANIMATION_DURATIONS.UP_CARD_PICKUP,
                });
                break;
            case 'DealerDiscarded':
                if (currentDeal && event.card && myPosition === event.position) {
                    currentDeal = {
                        ...currentDeal,
                        myHand: removeCardFromHand(currentDeal.myHand, event.card),
                    };
                } else if (currentDeal && myPosition !== event.position) {
                    currentDeal = {
                        ...currentDeal,
                        otherHandCounts: adjustOtherHandCount(currentDeal.otherHandCounts, event.position),
                    };
                }
                steps.push({
                    animation: EMPTY_ANIMATION,
                    applyState: currentDeal ? withDeal(rawState, currentDeal) : rawState,
                    eventIndex: event.eventIndex,
                    durationMs: 0,
                });
                break;
            case 'CardPlayed': {
                if (currentDeal && event.position === myPosition) {
                    currentDeal = {
                        ...currentDeal,
                        myHand: removeCardFromHand(currentDeal.myHand, event.card),
                    };
                } else if (currentDeal) {
                    currentDeal = {
                        ...currentDeal,
                        otherHandCounts: adjustOtherHandCount(currentDeal.otherHandCounts, event.position),
                    };
                }
                trickCards.push({ card: event.card, playerPosition: event.position });
                steps.push({
                    animation: { ...EMPTY_ANIMATION, overrideTrickCards: [...trickCards] },
                    applyState: currentDeal ? withDeal(rawState, currentDeal) : rawState,
                    eventIndex: event.eventIndex,
                    durationMs: ANIMATION_DURATIONS.CARD_PLAY,
                });
                break;
            }
            case 'TrickCompleted':
                trickCards.length = 0;
                steps.push({
                    animation: EMPTY_ANIMATION,
                    eventIndex: event.eventIndex,
                    durationMs: ANIMATION_DURATIONS.TRICK_CLEAR,
                });
                break;
            case 'DealCompleted':
                steps.push({
                    animation: {
                        ...EMPTY_ANIMATION,
                        dealResultMessage: buildDealResultMessage(
                            event.result,
                            event.winningTeam,
                            null,
                            0,
                        ),
                    },
                    eventIndex: event.eventIndex,
                    durationMs: ANIMATION_DURATIONS.DEAL_RESULT,
                });
                break;
            case 'GameCompleted':
            case 'WaitingForDecision':
                steps.push({
                    animation: EMPTY_ANIMATION,
                    eventIndex: event.eventIndex,
                    durationMs: 0,
                });
                break;
        }
    }

    return steps;
}

function buildBaseState(rawState: PlayerGameState, events: PlayerGameEvent[], startAfter: number): PlayerGameState {
    if (!rawState.currentDeal) return rawState;

    const newEvents = events.filter(e => e.eventIndex > startAfter);
    if (newEvents.length === 0) return rawState;

    const firstEvent = newEvents[0];
    if (firstEvent.eventType === 'NewDealStarted') {
        return rawState;
    }

    return { ...rawState, gameStatus: 'Playing' };
}

export function useEventAnimator(rawState: PlayerGameState | null) {
    const [displayState, setDisplayState] = useState<PlayerGameState | null>(null);
    const [animation, setAnimation] = useState<AnimationState>(EMPTY_ANIMATION);

    const lastAnimatedRef = useRef(-1);
    const initializedRef = useRef(false);
    const queueRef = useRef<AnimationStep[]>([]);
    const runningRef = useRef(false);
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const latestRawRef = useRef<PlayerGameState | null>(null);
    const [drainCount, setDrainCount] = useState(0);

    const drainRef = useRef<() => void>(() => {});

    useEffect(() => {
        drainRef.current = () => {
            if (queueRef.current.length === 0) {
                runningRef.current = false;
                setAnimation(EMPTY_ANIMATION);
                if (latestRawRef.current) {
                    setDisplayState(latestRawRef.current);
                }
                setDrainCount(c => c + 1);
                return;
            }
            runningRef.current = true;
            const step = queueRef.current.shift()!;
            setAnimation(step.animation);
            if (step.applyState) {
                setDisplayState(step.applyState);
            }
            lastAnimatedRef.current = step.eventIndex;
            if (step.durationMs > 0) {
                timerRef.current = setTimeout(() => drainRef.current(), step.durationMs);
            } else {
                drainRef.current();
            }
        };
        return () => { if (timerRef.current) clearTimeout(timerRef.current); };
    }, []);

    const enqueue = useCallback((steps: AnimationStep[]) => {
        queueRef.current.push(...steps);
        if (!runningRef.current) drainRef.current();
    }, []);

    useEffect(() => {
        if (!rawState) {
            latestRawRef.current = null;
            lastAnimatedRef.current = -1;
            initializedRef.current = false;
            enqueue([{ animation: EMPTY_ANIMATION, applyState: undefined, eventIndex: -1, durationMs: 0 }]);
            return;
        }

        latestRawRef.current = rawState;
        const events = rawState.events ?? [];

        if (!initializedRef.current) {
            initializedRef.current = true;
            if (events.length > 0) {
                lastAnimatedRef.current = events[events.length - 1].eventIndex;
            }
            enqueue([{ animation: EMPTY_ANIMATION, applyState: rawState, eventIndex: lastAnimatedRef.current, durationMs: 0 }]);
            return;
        }

        const steps = buildSteps(events, lastAnimatedRef.current, rawState);

        if (steps.length > 0) {
            const baseState = buildBaseState(rawState, events, lastAnimatedRef.current);
            if (!steps[0].applyState) {
                steps[0] = { ...steps[0], applyState: baseState };
            }
            enqueue(steps);
        } else if (!runningRef.current) {
            enqueue([{ animation: EMPTY_ANIMATION, applyState: rawState, eventIndex: lastAnimatedRef.current, durationMs: 0 }]);
        }
    }, [rawState, drainCount, enqueue]);

    return { displayState, animationState: animation };
}
