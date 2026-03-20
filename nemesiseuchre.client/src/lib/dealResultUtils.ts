import type { DealResult, PlayerPosition, Team } from '@/types/game';
import { getTeamForPosition, getTeamName } from './teamUtils';

export function buildDealResultMessage(
    result: DealResult,
    winningTeam: Team,
    callingPlayer: PlayerPosition | null,
    completedTrickCount: number,
): string {
    const teamName = getTeamName(winningTeam);
    const callingTeam = callingPlayer ? getTeamForPosition(callingPlayer) : null;

    switch (result) {
        case 'WonAndWentAlone':
            return `${teamName} goes alone and sweeps! (+4)`;
        case 'WonGotAllTricks':
            return `${teamName} marches for all 5 tricks! (+2)`;
        case 'OpponentsEuchred': {
            const euchredTeamName = callingTeam ? getTeamName(callingTeam) : 'Calling team';
            return `${euchredTeamName} got euchred! ${teamName} scores (+2)`;
        }
        case 'WonStandardBid':
            return `${teamName} wins with ${completedTrickCount} tricks (+1)`;
        case 'ThrowIn':
            return 'Throw-in — no one called trump';
    }
}
