import type { CallTrumpDecision } from '@/types/game';

export const TRUMP_DECISION_LABELS: Record<CallTrumpDecision, string> = {
    Pass: 'Pass',
    OrderItUp: 'Order It Up',
    OrderItUpAndGoAlone: 'Order It Up (Alone)',
    CallSpades: 'Call Spades',
    CallSpadesAndGoAlone: 'Call Spades (Alone)',
    CallHearts: 'Call Hearts',
    CallHeartsAndGoAlone: 'Call Hearts (Alone)',
    CallClubs: 'Call Clubs',
    CallClubsAndGoAlone: 'Call Clubs (Alone)',
    CallDiamonds: 'Call Diamonds',
    CallDiamondsAndGoAlone: 'Call Diamonds (Alone)',
};

export const DEALER_TRUMP_DECISION_LABELS: Partial<Record<CallTrumpDecision, string>> = {
    OrderItUp: 'Pick It Up',
    OrderItUpAndGoAlone: 'Pick It Up (Alone)',
};

export const TRUMP_BUBBLE_LABELS: Record<CallTrumpDecision, string> = {
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
