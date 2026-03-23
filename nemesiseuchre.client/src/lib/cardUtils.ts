import type { Card, Rank, Suit } from '@/types/game';

export function suitSymbol(suit: Suit): string {
    switch (suit) {
        case 'Spades': return '♠';
        case 'Clubs': return '♣';
        case 'Hearts': return '♥';
        case 'Diamonds': return '♦';
    }
}

export function suitColor(suit: Suit): string {
    return suit === 'Hearts' || suit === 'Diamonds' ? 'text-red-500' : 'text-foreground';
}

export function rankLabel(rank: Rank): string {
    switch (rank) {
        case 'Nine': return '9';
        case 'Ten': return '10';
        case 'Jack': return 'J';
        case 'Queen': return 'Q';
        case 'King': return 'K';
        case 'Ace': return 'A';
    }
}

export function isCardValid(card: Card, validCards?: Card[]): boolean {
    if (!validCards) return true;
    return validCards.some(vc => vc.suit === card.suit && vc.rank === card.rank);
}

const SAME_COLOR: Record<Suit, Suit> = {
    Spades: 'Clubs',
    Clubs: 'Spades',
    Hearts: 'Diamonds',
    Diamonds: 'Hearts',
};

const RANK_VALUE: Record<Rank, number> = {
    Nine: 0, Ten: 1, Jack: 2, Queen: 3, King: 4, Ace: 5,
};

function cardSortValue(card: Card, trump: Suit): number {
    const partnerSuit = SAME_COLOR[trump];

    if (card.rank === 'Jack' && card.suit === trump) return 200;
    if (card.rank === 'Jack' && card.suit === partnerSuit) return 199;
    if (card.suit === trump) return 100 + RANK_VALUE[card.rank];

    const suitOrder = card.suit === partnerSuit ? 2
        : card.suit < trump ? 1 : 0;
    return suitOrder * 10 + RANK_VALUE[card.rank];
}

export function sortHandByTrump(hand: Card[], trump: Suit | null | undefined): Card[] {
    if (!trump) return hand;
    return [...hand].sort((a, b) => cardSortValue(b, trump) - cardSortValue(a, trump));
}
