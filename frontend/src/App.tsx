import React from 'react';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import { SignInPage } from './SignInPage';
import { HomePage } from './HomePage';
import { GamePage } from './GamePage';
import { NotFoundPage } from './NotFoundPage';
import { Header } from './Header';
import { GamesPage } from './GamesPage';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <div className="App">
        <Header />
        <Switch>
          <Route exact path="/" component={HomePage} />
          <Route exact path="/games/:userName" component={GamesPage} />
          <Route path="/signin" component={SignInPage} />
          <Route path="/games/:userName/:gameId" component={GamePage} />
          <Route component={NotFoundPage} />
        </Switch>
      </div>
    </BrowserRouter>
  );
}

export default App;
