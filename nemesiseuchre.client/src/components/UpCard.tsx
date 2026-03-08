import type { Card, PlayerPosition } from '@/types/game';
import { PlayingCard } from './PlayingCard';

interface Props {
    card: Card | null;
    dealerPosition: PlayerPosition;
}

const positionClasses: Record<PlayerPosition, string> = {
    North: 'absolute top-[290px] left-1/2 -translate-x-1/2',
    South: 'absolute bottom-[290px] left-1/2 -translate-x-1/2',
    East: 'absolute right-[290px] top-1/2 -translate-y-1/2',
    West: 'absolute left-[290px] top-1/2 -translate-y-1/2',
};

export function UpCard({ card, dealerPosition }: Props) {
    return (
        <div className={`${positionClasses[dealerPosition]} z-30`}>
            <PlayingCard card={card} />
        </div>
    );
}
