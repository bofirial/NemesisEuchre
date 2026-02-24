import { Link, Route, Routes } from 'react-router-dom';
import { AppFooter } from '@/components/AppFooter';
import { NavLoginButton } from '@/components/LoginButton';
import { GameLobbyPage } from '@/pages/GameLobbyPage';
import { HomePage } from '@/pages/HomePage';

function App() {
    return (
        <div className="flex min-h-svh flex-col bg-background text-foreground">
            <header className="sticky top-0 z-50 border-b border-border bg-background/95 backdrop-blur-sm">
                <div className="mx-auto flex h-14 max-w-5xl items-center justify-between px-4">
                    <Link to="/" className="flex items-center gap-2">
                        <img src="/nemesiseuchreLogo.svg" alt="NemesisEuchre" className="h-8 w-8" />
                        <span className="font-semibold tracking-tight">
                            <span className="text-brand-blue">Nemesis</span>
                            <span className="text-brand-red">Euchre</span>
                        </span>
                    </Link>
                    <NavLoginButton />
                </div>
            </header>

            <main className="mx-auto max-w-5xl flex-1 px-4 py-24 text-center">
                <Routes>
                    <Route path="/" element={<HomePage />} />
                    <Route path="/game/:gameName" element={<GameLobbyPage />} />
                </Routes>
            </main>
            <AppFooter />
        </div>
    );
}

export default App;
