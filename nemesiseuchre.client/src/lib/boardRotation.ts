import type { PlayerPosition } from '@/types/game';

export type ScreenPosition = PlayerPosition;

const RING: PlayerPosition[] = ['North', 'East', 'South', 'West'];

const OFFSET: Record<PlayerPosition, number> = {
    South: 0,
    East: 1,
    North: 2,
    West: 3,
};

export function getPartnerPosition(position: PlayerPosition): PlayerPosition {
    const idx = RING.indexOf(position);
    return RING[(idx + 2) % 4];
}

export function toScreenPosition(gamePosition: PlayerPosition, myPosition: PlayerPosition): ScreenPosition {
    const idx = RING.indexOf(gamePosition);
    return RING[(idx + OFFSET[myPosition]) % 4];
}
