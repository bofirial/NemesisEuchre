export type PlayerPosition = 'North' | 'East' | 'South' | 'West';
export type GameStatusViewModel = 'Lobby' | 'Playing';
export type Suit = 'Spades' | 'Clubs' | 'Hearts' | 'Diamonds';
export type Rank = 'Nine' | 'Ten' | 'Jack' | 'Queen' | 'King' | 'Ace';
export type Team = 'Team1' | 'Team2';
export type DealStatus = 'NotStarted' | 'SelectingTrump' | 'Playing' | 'Scoring' | 'Complete';

export interface Card { suit: Suit; rank: Rank; }
export interface PlayedCard { card: Card; playerPosition: PlayerPosition; }

export interface PlayerInfo {
    position: PlayerPosition;
    name: string;
    team: Team;
}

export interface CompletedTrickInfo {
    trickNumber: number;
    leadPosition: PlayerPosition;
    cardsPlayed: PlayedCard[];
    winningPosition: PlayerPosition;
    winningTeam: Team;
}

export interface DealState {
    dealStatus: DealStatus;
    dealerPosition: PlayerPosition | null;
    upCard: Card | null;
    trump: Suit | null;
    callingPlayer: PlayerPosition | null;
    callingPlayerIsGoingAlone: boolean;
    myHand: Card[];
    otherHandCounts: Partial<Record<PlayerPosition, number>>;
    completedTricks: CompletedTrickInfo[];
}

export interface PlayerGameState {
    gameName: string;
    gameStatus: GameStatusViewModel;
    myPosition: PlayerPosition;
    players: Partial<Record<PlayerPosition, PlayerInfo>>;
    team1Score: number;
    team2Score: number;
    currentDeal: DealState | null;
}
