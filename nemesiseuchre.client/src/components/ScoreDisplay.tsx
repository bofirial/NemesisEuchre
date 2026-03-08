interface ScoreDisplayProps {
    teamName: string;
    score: number;
}

export function ScoreDisplay({ teamName, score }: ScoreDisplayProps) {
    return (
        <div className="flex flex-col items-center gap-0.5">
            <span className="text-[10px] uppercase tracking-widest font-medium text-muted-foreground/70">
                {teamName}
            </span>
            <span className="text-2xl font-bold">{score}</span>
        </div>
    );
}
