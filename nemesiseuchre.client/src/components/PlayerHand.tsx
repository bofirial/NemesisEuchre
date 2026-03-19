import type { Card, PlayerPosition } from '@/types/game';
import { PlayingCard } from './PlayingCard';

interface PlayerHandProps {
    cards: Card[] | null;
    count: number;
    position: PlayerPosition;
    onCardClick?: (card: Card) => void;
    highlightCards?: boolean;
}

const rotationByPosition: Record<PlayerPosition, string> = {
    South: 'rotate-0',
    North: 'rotate-180',
    East: '-rotate-90',
    West: 'rotate-90',
};

export function PlayerHand({ cards, count, position, onCardClick, highlightCards }: PlayerHandProps) {
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

                    return (
                        <div
                            key={i}
                            className={`absolute left-1/2 top-0 ${
                                highlightCards && card ? 'cursor-pointer hover:scale-110 transition-transform' : ''
                            }`}
                            style={{ transform: `translateX(calc(-50% + ${dx}px)) translateY(${dy}px) rotate(${angle}deg)` }}
                            onClick={highlightCards && card && onCardClick ? () => onCardClick(card) : undefined}
                        >
                            <PlayingCard card={card} />
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
