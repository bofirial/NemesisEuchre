import type { PlayerPosition, Team } from '@/types/game';

export function getTeamForPosition(position: PlayerPosition): Team {
    return position === 'North' || position === 'South' ? 'Team1' : 'Team2';
}

export function getTeamLabel(position: PlayerPosition): string {
    return getTeamForPosition(position) === 'Team1' ? 'Team 1' : 'Team 2';
}

export function getTeamName(team: Team): string {
    return team === 'Team1' ? 'Team 1' : 'Team 2';
}
