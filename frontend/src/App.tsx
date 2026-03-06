import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { SignInPage } from './SignInPage';
import { HomePage } from './HomePage';
import { GamePage } from './GamePage';
import { NotFoundPage } from './NotFoundPage';
import { Header } from './Header';
import { GamesPage } from './GamesPage';
import { RegisterPage } from './RegisterPage';
import { Flex, Box } from '@radix-ui/themes';
import './App.css';



const config = window.APP_CONFIG || { basePath: '/' };


function App() {
  return (
    <BrowserRouter basename={config.basePath}>
      <Flex direction="column" minHeight="100vh" className="app-root">
        <Header />

        {/* Main Content Area */}
        {/* We add 'position: relative' to ensure z-index works correctly for children */}
        <Box style={{ position: 'relative', zIndex: 1 }} p={{ initial: '4', md: '6' }}>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/games/:userName" element={<GamesPage />} />
            <Route path="/games/:userName/:page" element={<GamesPage />} />
            <Route path="/signin" element={<SignInPage />} />
            <Route path="/register" element={<RegisterPage />} />
            <Route path="/game/:userName/:gameId" element={<GamePage />} />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </Box>
      </Flex>
    </BrowserRouter>
  );
}

export default App;
