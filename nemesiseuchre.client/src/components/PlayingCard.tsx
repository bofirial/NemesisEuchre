import { motion } from 'framer-motion';
import type { Card } from '@/types/game';
import { rankLabel, suitSymbol } from '@/lib/cardUtils';
import { cn } from '@/lib/utils';

interface PlayingCardProps {
    card: Card | null;
    selected?: boolean;
    className?: string;
    small?: boolean;
}

function CardBack({ small }: { small: boolean }) {
    return (
        <div className="absolute inset-0 rounded-lg bg-primary flex items-center justify-center overflow-hidden">
            <div className="absolute inset-1 rounded-md border border-primary-foreground/20" />
            <div className="flex flex-col items-center gap-0.5 select-none">
                <span className={cn('font-bold tracking-tight text-brand-blue', small ? 'text-[6px]' : 'text-[9px]')}>Nemesis</span>
                <span className={cn('font-bold tracking-tight text-brand-red', small ? 'text-[6px]' : 'text-[9px]')}>Euchre</span>
            </div>
        </div>
    );
}

function CardFace({ card, small }: { card: Card; small: boolean }) {
    const rank = rankLabel(card.rank);
    const suit = suitSymbol(card.suit);
    const isRed = card.suit === 'Hearts' || card.suit === 'Diamonds';
    const color = isRed ? 'text-red-400' : 'text-primary-foreground';

    return (
        <div className={cn('absolute inset-0 rounded-lg bg-primary flex flex-col select-none', small ? 'p-0.5' : 'p-1')}>
            <div className={cn('flex flex-col items-start leading-none', color)}>
                <span className={cn('font-bold', small ? 'text-[8px]' : 'text-sm')}>{rank}</span>
                <span className={small ? 'text-[7px]' : 'text-xs'}>{suit}</span>
            </div>
            <div className={cn('flex-1 flex items-center justify-center overflow-hidden', color, small ? 'text-sm' : 'text-xl')}>
                {suit}
            </div>
            <div className={cn('flex flex-col items-start leading-none rotate-180', color)}>
                <span className={cn('font-bold', small ? 'text-[8px]' : 'text-sm')}>{rank}</span>
                <span className={small ? 'text-[7px]' : 'text-xs'}>{suit}</span>
            </div>
        </div>
    );
}

export function PlayingCard({ card, selected = false, className, small = false }: PlayingCardProps) {
    const layoutId = card ? `card-${card.suit}-${card.rank}` : undefined;

    return (
        <motion.div
            layoutId={layoutId}
            className={cn(
                'relative rounded-lg border-2 shadow-md transition-transform duration-150 overflow-hidden',
                small ? 'w-10 h-14' : 'w-16 h-24',
                selected ? 'border-primary -translate-y-2' : 'border-border',
                className,
            )}
        >
            {card ? <CardFace card={card} small={small} /> : <CardBack small={small} />}
        </motion.div>
    );
}
