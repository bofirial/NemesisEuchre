import type { PlayerGameState, PlayerPosition, SeatOccupant } from '@/types/game';

export function botDisplayName(seat: SeatOccupant): string {
    if (seat.botModelName) return seat.botModelName;
    if (seat.botActorType === 'Chaos') return 'ChaosBot';
    if (seat.botActorType === 'Beta') return 'BetaBot';
    if (seat.botActorType === 'Chad') return 'ChadBot';
    return 'Bot';
}

export function seatDisplayName(position: PlayerPosition, seats: PlayerGameState['seats']): string {
    const occupant = seats[position];
    if (!occupant) return position;
    if (occupant.gitHubLogin) return occupant.gitHubLogin;
    return botDisplayName(occupant);
}
