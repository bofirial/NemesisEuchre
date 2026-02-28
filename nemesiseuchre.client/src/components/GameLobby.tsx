import { Star } from 'lucide-react';
import type { PlayerGameState } from '@/types/game';

interface Props { gameState: PlayerGameState; }

export function GameLobby({ gameState }: Props) {
    return (
        <div className="flex flex-col items-center gap-4">
            <h2 className="text-2xl font-semibold">Game Lobby — {gameState.sessionName}</h2>
            <ul className="flex flex-col gap-1">
                {gameState.connectedUsers.map(u => (
                    <li key={u.gitHubLogin} className="flex items-center gap-2 text-sm">
                        {u.isSessionLeader && <Star className="size-4 fill-yellow-400 text-yellow-400" />}
                        <span>{u.gitHubLogin}</span>
                    </li>
                ))}
            </ul>
        </div>
    );
}
