export type PlayerPosition = 'North' | 'East' | 'South' | 'West';
export type GameStatusViewModel = 'Lobby' | 'Playing';
export type Suit = 'Spades' | 'Clubs' | 'Hearts' | 'Diamonds';
export type Rank = 'Nine' | 'Ten' | 'Jack' | 'Queen' | 'King' | 'Ace';
export type Team = 'Team1' | 'Team2';
export type DealStatus = 'NotStarted' | 'SelectingTrumpPhase1' | 'SelectingTrumpPhase2' | 'Playing' | 'Scoring' | 'Complete' | 'DealerDiscarding';

export type CallTrumpDecision =
    | 'Pass'
    | 'OrderItUp' | 'OrderItUpAndGoAlone'
    | 'CallSpades' | 'CallSpadesAndGoAlone'
    | 'CallHearts' | 'CallHeartsAndGoAlone'
    | 'CallClubs' | 'CallClubsAndGoAlone'
    | 'CallDiamonds' | 'CallDiamondsAndGoAlone';

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
    currentTrickCards: PlayedCard[];
    currentDeciderPosition: PlayerPosition | null;
    validTrumpDecisions: CallTrumpDecision[] | null;
    validDiscardCards: Card[] | null;
    validCardsToPlay: Card[] | null;
}

export interface ConnectedUserInfo {
    gitHubLogin: string;
    isSessionLeader: boolean;
}

export type ActorType = 'Chaos' | 'Beta' | 'Chad' | 'Model';

export interface SeatOccupant {
    gitHubLogin: string | null;
    botActorType: ActorType | null;
    botModelName: string | null;
}

export interface PlayerGameState {
    sessionName: string;
    gameStatus: GameStatusViewModel;
    myPosition: PlayerPosition;
    players: Partial<Record<PlayerPosition, PlayerInfo>>;
    team1Score: number;
    team2Score: number;
    connectedUsers: ConnectedUserInfo[];
    seats: Partial<Record<PlayerPosition, SeatOccupant>>;
    currentDeal: DealState | null;
}
