import { useCallback, useEffect, useRef, useState } from 'react';
import type { CallTrumpDecision, DealResult, PlayedCard, PlayerGameState, PlayerPosition, Team } from '@/types/game';

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

const TRUMP_BUBBLE_LABELS: Record<CallTrumpDecision, string> = {
    Pass: 'Pass',
    OrderItUp: 'Order It Up',
    OrderItUpAndGoAlone: 'Order It Up Alone!',
    CallSpades: 'Spades',
    CallSpadesAndGoAlone: 'Spades, Alone!',
    CallHearts: 'Hearts',
    CallHeartsAndGoAlone: 'Hearts, Alone!',
    CallClubs: 'Clubs',
    CallClubsAndGoAlone: 'Clubs, Alone!',
    CallDiamonds: 'Diamonds',
    CallDiamondsAndGoAlone: 'Diamonds, Alone!',
};

function buildDealResultMessage(
    result: DealResult,
    winningTeam: Team,
    callingPlayer: PlayerPosition | null,
    completedTrickCount: number,
): string {
    const teamName = winningTeam === 'Team1' ? 'Team 1' : 'Team 2';
    const callingTeam = callingPlayer === 'North' || callingPlayer === 'South' ? 'Team1' : 'Team2';

    switch (result) {
        case 'WonAndWentAlone':
            return `${teamName} goes alone and sweeps! (+4)`;
        case 'WonGotAllTricks':
            return `${teamName} marches for all 5 tricks! (+2)`;
        case 'OpponentsEuchred': {
            const euchredTeam = callingTeam === 'Team1' ? 'Team 1' : 'Team 2';
            return `${euchredTeam} got euchred! ${teamName} scores (+2)`;
        }
        case 'WonStandardBid':
            return `${teamName} wins with ${completedTrickCount} tricks (+1)`;
        case 'ThrowIn':
            return 'Throw-in — no one called trump';
    }
}

interface QueuedStep {
    animation: AnimationState;
    applyState?: PlayerGameState;
    durationMs: number;
}

export function useAnimationQueue(rawState: PlayerGameState | null) {
    const [displayState, setDisplayState] = useState<PlayerGameState | null>(null);
    const [animation, setAnimation] = useState<AnimationState>(EMPTY_ANIMATION);

    const seenTrumpRef = useRef(0);
    const seenTrickCardsRef = useRef(0);
    const seenCompletedRef = useRef(0);
    const seenDealResultRef = useRef(false);
    const lastDealerRef = useRef<PlayerPosition | null>(null);

    const queueRef = useRef<QueuedStep[]>([]);
    const runningRef = useRef(false);
    const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
    const [drainCount, setDrainCount] = useState(0);

    const drainRef = useRef<() => void>(() => {});

    useEffect(() => {
        drainRef.current = () => {
            if (queueRef.current.length === 0) {
                runningRef.current = false;
                setDrainCount(c => c + 1);
                return;
            }
            runningRef.current = true;
            const step = queueRef.current.shift()!;
            setAnimation(step.animation);
            if (step.applyState) setDisplayState(step.applyState);
            if (step.durationMs > 0) {
                timerRef.current = setTimeout(() => drainRef.current(), step.durationMs);
            } else {
                drainRef.current();
            }
        };
        return () => { if (timerRef.current) clearTimeout(timerRef.current); };
    }, []);

    const enqueue = useCallback((steps: QueuedStep[]) => {
        queueRef.current.push(...steps);
        if (!runningRef.current) drainRef.current();
    }, []);

    useEffect(() => {
        if (!rawState) {
            enqueue([{ animation: EMPTY_ANIMATION, applyState: undefined, durationMs: 0 }]);
            seenTrumpRef.current = 0;
            seenTrickCardsRef.current = 0;
            seenCompletedRef.current = 0;
            seenDealResultRef.current = false;
            lastDealerRef.current = null;
            return;
        }

        const deal = rawState.currentDeal;

        if (!deal) {
            seenTrumpRef.current = 0;
            seenTrickCardsRef.current = 0;
            seenCompletedRef.current = 0;
            seenDealResultRef.current = false;
            lastDealerRef.current = null;
            enqueue([{ animation: EMPTY_ANIMATION, applyState: rawState, durationMs: 0 }]);
            return;
        }

        if (deal.dealerPosition !== lastDealerRef.current && lastDealerRef.current !== null) {
            if (runningRef.current || queueRef.current.length > 0) {
                return;
            }
            seenTrumpRef.current = 0;
            seenTrickCardsRef.current = 0;
            seenCompletedRef.current = 0;
            seenDealResultRef.current = false;
            lastDealerRef.current = deal.dealerPosition;
        }
        if (lastDealerRef.current === null) {
            lastDealerRef.current = deal.dealerPosition;
        }

        const steps: QueuedStep[] = [];

        const trumps = deal.trumpDecisions ?? [];
        for (let i = seenTrumpRef.current; i < trumps.length; i++) {
            const d = trumps[i];
            steps.push({
                animation: { ...EMPTY_ANIMATION, activeBubble: { position: d.position, text: TRUMP_BUBBLE_LABELS[d.decision] } },
                durationMs: 1000,
            });
        }
        if (trumps.length > seenTrumpRef.current) {
            const allPhase1Passed = trumps.length >= 4 && trumps.slice(0, 4).every(d => d.decision === 'Pass');
            if (allPhase1Passed && seenTrumpRef.current < 4) {
                steps.push({ animation: { ...EMPTY_ANIMATION, upCardAnimating: 'flipping' }, durationMs: 1000 });
            }
            const last = trumps[trumps.length - 1];
            if (last.decision === 'OrderItUp' || last.decision === 'OrderItUpAndGoAlone') {
                steps.push({ animation: { ...EMPTY_ANIMATION, upCardAnimating: 'pickup' }, durationMs: 1000 });
            }
            seenTrumpRef.current = trumps.length;
        }

        const completedTricks = deal.completedTricks ?? [];
        const completedCount = completedTricks.length;
        const trickCards = deal.currentTrickCards ?? [];

        if (completedCount > seenCompletedRef.current) {
            for (let t = seenCompletedRef.current; t < completedCount; t++) {
                const trick = completedTricks[t];
                for (let c = (t === seenCompletedRef.current ? seenTrickCardsRef.current : 0); c < trick.cardsPlayed.length; c++) {
                    steps.push({
                        animation: { ...EMPTY_ANIMATION, overrideTrickCards: trick.cardsPlayed.slice(0, c + 1) },
                        durationMs: 1000,
                    });
                }
                steps.push({ animation: EMPTY_ANIMATION, durationMs: 1200 });
                steps.push({ animation: EMPTY_ANIMATION, applyState: rawState, durationMs: 0 });
            }
            seenTrickCardsRef.current = 0;
            seenCompletedRef.current = completedCount;
        }

        if (trickCards.length > seenTrickCardsRef.current) {
            for (let c = seenTrickCardsRef.current; c < trickCards.length; c++) {
                steps.push({
                    animation: { ...EMPTY_ANIMATION, overrideTrickCards: trickCards.slice(0, c + 1) },
                    durationMs: 1000,
                });
            }
            seenTrickCardsRef.current = trickCards.length;
        }

        if (deal.dealResult && !seenDealResultRef.current) {
            const msg = buildDealResultMessage(deal.dealResult, deal.winningTeam!, deal.callingPlayer, completedTricks.length);
            steps.push({ animation: { ...EMPTY_ANIMATION, dealResultMessage: msg }, applyState: rawState, durationMs: 2500 });
            seenDealResultRef.current = true;
        }

        if (steps.length > 0) {
            steps.push({ animation: EMPTY_ANIMATION, applyState: rawState, durationMs: 0 });
            enqueue(steps);
        } else if (runningRef.current) {
            const lastStep = queueRef.current[queueRef.current.length - 1];
            if (lastStep) lastStep.applyState = rawState;
            else queueRef.current.push({ animation: EMPTY_ANIMATION, applyState: rawState, durationMs: 0 });
        } else {
            enqueue([{ animation: EMPTY_ANIMATION, applyState: rawState, durationMs: 0 }]);
        }
    }, [rawState, drainCount, enqueue]);

    return { displayState, animationState: animation };
}
