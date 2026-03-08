import type { Card, PlayerPosition } from '@/types/game';
import { PlayingCard } from './PlayingCard';

interface PlayerHandProps {
    cards: Card[] | null;
    count: number;
    position: PlayerPosition;
}

const rotationByPosition: Record<PlayerPosition, string> = {
    South: 'rotate-0',
    North: 'rotate-180',
    East: '-rotate-90',
    West: 'rotate-90',
};

export function PlayerHand({ cards, count, position }: PlayerHandProps) {
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

                    return (
                        <div
                            key={i}
                            className="absolute left-1/2 top-0"
                            style={{ transform: `translateX(calc(-50% + ${dx}px)) translateY(${dy}px) rotate(${angle}deg)` }}
                        >
                            <PlayingCard card={cards !== null ? cards[i] : null} />
                        </div>
                    );
                })}
            </div>
        </div>
    );
}
