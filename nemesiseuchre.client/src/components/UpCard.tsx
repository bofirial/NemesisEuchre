import type { Card, PlayerPosition } from '@/types/game';
import { cn } from '@/lib/utils';
import { PlayingCard } from './PlayingCard';

interface Props {
    card: Card | null;
    dealerPosition: PlayerPosition;
    animating?: 'flipping' | 'pickup' | null;
}

const positionClasses: Record<PlayerPosition, string> = {
    North: 'absolute top-[290px] left-1/2 -translate-x-1/2',
    South: 'absolute bottom-[290px] left-1/2 -translate-x-1/2',
    East: 'absolute right-[290px] top-1/2 -translate-y-1/2',
    West: 'absolute left-[290px] top-1/2 -translate-y-1/2',
};

export function UpCard({ card, dealerPosition, animating }: Props) {
    return (
        <div className={cn(
            positionClasses[dealerPosition],
            'z-30 transition-all duration-700 ease-in-out',
            animating === 'flipping' && '[transform:rotateY(90deg)]',
            animating === 'pickup' && 'scale-0 opacity-0',
        )}>
            <PlayingCard card={card} />
        </div>
    );
}
