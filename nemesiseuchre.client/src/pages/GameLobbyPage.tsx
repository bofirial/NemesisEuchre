import { useParams } from 'react-router-dom';

export function GameLobbyPage() {
    const { gameName } = useParams<{ gameName: string }>();
    return (
        <div className="flex flex-col items-center gap-6">
            <h1 className="text-4xl font-bold tracking-tight">{gameName}</h1>
            <p className="text-muted-foreground">Game Lobby coming soon.</p>
        </div>
    );
}
