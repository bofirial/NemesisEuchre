import { motion } from 'framer-motion';
import type { Card } from '@/types/game';
import { rankLabel, suitColor, suitSymbol } from '@/lib/cardUtils';
import { cn } from '@/lib/utils';

interface PlayingCardProps {
    card: Card | null;
    selected?: boolean;
    className?: string;
}

function CardBack() {
    return (
        <div className="absolute inset-0 rounded-lg bg-primary flex items-center justify-center overflow-hidden">
            <div className="absolute inset-1 rounded-md border border-primary-foreground/20" />
            <div className="flex flex-col items-center gap-0.5 select-none">
                <span className="text-[9px] font-bold tracking-tight text-primary-foreground/70">Nemesis</span>
                <span className="text-[9px] font-bold tracking-tight text-primary-foreground/70">Euchre</span>
            </div>
        </div>
    );
}

function CardFace({ card }: { card: Card }) {
    const rank = rankLabel(card.rank);
    const suit = suitSymbol(card.suit);
    const color = suitColor(card.suit);

    return (
        <div className="absolute inset-0 rounded-lg bg-card flex flex-col p-1 select-none">
            <div className={cn('flex flex-col items-start leading-none', color)}>
                <span className="text-sm font-bold">{rank}</span>
                <span className="text-xs">{suit}</span>
            </div>
            <div className={cn('flex-1 flex items-center justify-center text-2xl', color)}>
                {suit}
            </div>
            <div className={cn('flex flex-col items-end leading-none rotate-180', color)}>
                <span className="text-sm font-bold">{rank}</span>
                <span className="text-xs">{suit}</span>
            </div>
        </div>
    );
}

export function PlayingCard({ card, selected = false, className }: PlayingCardProps) {
    const layoutId = card ? `card-${card.suit}-${card.rank}` : undefined;

    return (
        <motion.div
            layoutId={layoutId}
            className={cn(
                'relative w-16 h-24 rounded-lg border-2 shadow-md transition-transform duration-150',
                selected ? 'border-primary -translate-y-2' : 'border-border',
                className,
            )}
        >
            {card ? <CardFace card={card} /> : <CardBack />}
        </motion.div>
    );
}
