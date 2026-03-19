import type { Card, PlayerPosition } from '@/types/game';
import { PlayingCard } from './PlayingCard';

interface PlayerHandProps {
    cards: Card[] | null;
    count: number;
    position: PlayerPosition;
    onCardClick?: (card: Card) => void;
    highlightCards?: boolean;
    validCards?: Card[];
}

const rotationByPosition: Record<PlayerPosition, string> = {
    South: 'rotate-0',
    North: 'rotate-180',
    East: '-rotate-90',
    West: 'rotate-90',
};

function isCardValid(card: Card, validCards?: Card[]): boolean {
    if (!validCards) return true;
    return validCards.some(vc => vc.suit === card.suit && vc.rank === card.rank);
}

export function PlayerHand({ cards, count, position, onCardClick, highlightCards, validCards }: PlayerHandProps) {
    const cardCount = cards !== null ? cards.length : count;

    if (cardCount === 0) return null;

    const center = (cardCount - 1) / 2;

    return (
        <div className={rotationByPosition[position]}>
            <div className="relative w-52 h-32">
                {Array.from({ length: cardCount }, (_, i) => {
                    const offset = i - center;
                    const dx = offset * 28;
                    const dy = Math.abs(offset) * 6;
                    const angle = offset * 6;
                    const card = cards !== null ? cards[i] : null;
                    const canPlay = highlightCards && card && isCardValid(card, validCards);

                    return (
                        <div
                            key={i}
                            className={`absolute left-1/2 top-0 ${
                                canPlay ? 'cursor-pointer hover:scale-110 transition-transform' : ''
                            } ${highlightCards && card && !canPlay ? 'opacity-40' : ''}`}
                            style={{ transform: `translateX(calc(-50% + ${dx}px)) translateY(${dy}px) rotate(${angle}deg)` }}
                            onClick={canPlay && onCardClick ? () => onCardClick(card) : undefined}
                        >
                            <PlayingCard card={card} />
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
