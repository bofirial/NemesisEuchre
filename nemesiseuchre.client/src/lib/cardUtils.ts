import type { Rank, Suit } from '@/types/game';

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
