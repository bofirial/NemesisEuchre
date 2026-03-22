import { useCallback, useEffect, useRef, useState } from 'react';
import type { PlayedCard, PlayerGameEvent, PlayerGameState, PlayerPosition } from '@/types/game';
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
    eventIndex: number;
    durationMs: number;
}

function buildSteps(events: PlayerGameEvent[], startAfter: number): AnimationStep[] {
    const steps: AnimationStep[] = [];
    const trickCards: PlayedCard[] = [];

    for (const event of events) {
        if (event.eventIndex <= startAfter) continue;

        switch (event.eventType) {
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
                steps.push({
                    animation: { ...EMPTY_ANIMATION, upCardAnimating: 'pickup' },
                    eventIndex: event.eventIndex,
                    durationMs: ANIMATION_DURATIONS.UP_CARD_PICKUP,
                });
                break;
            case 'CardPlayed':
                trickCards.push({ card: event.card, playerPosition: event.position });
                steps.push({
                    animation: { ...EMPTY_ANIMATION, overrideTrickCards: [...trickCards] },
                    eventIndex: event.eventIndex,
                    durationMs: ANIMATION_DURATIONS.CARD_PLAY,
                });
                break;
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
            case 'NewDealStarted':
            case 'GameCompleted':
            case 'WaitingForDecision':
            case 'DealerDiscarded':
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
            setDisplayState(null);
            setAnimation(EMPTY_ANIMATION);
            lastAnimatedRef.current = -1;
            initializedRef.current = false;
            return;
        }

        latestRawRef.current = rawState;
        const events = rawState.events ?? [];

        if (!initializedRef.current) {
            initializedRef.current = true;
            if (events.length > 0) {
                lastAnimatedRef.current = events[events.length - 1].eventIndex;
            }
            setDisplayState(rawState);
            return;
        }

        const steps = buildSteps(events, lastAnimatedRef.current);

        if (steps.length > 0) {
            enqueue(steps);
        } else if (!runningRef.current) {
            setDisplayState(rawState);
        }
    }, [rawState, drainCount, enqueue]);

    return { displayState, animationState: animation };
}
