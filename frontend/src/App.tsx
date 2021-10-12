import React from 'react';
import { BrowserRouter, Route, Switch } from 'react-router-dom';
import { SignInPageWithRouter as SignInPage } from './SignInPage';
import { HomePage } from './HomePage';
import { GamePage } from './GamePage';
import { NotFoundPage } from './NotFoundPage';
import { Header } from './Header';
import { GamesPage } from './GamesPage';
import { RegisterPageWithRouter as RegisterPage } from './RegisterPage';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <div className="App">
        <Header />
        <Switch>
          <Route exact path="/" component={HomePage} />
          <Route exact path="/games/:userName" component={GamesPage} />
          <Route exact path="/games/:userName/:page" component={GamesPage} />
          <Route path="/signin" component={SignInPage} />
          <Route path="/register" component={RegisterPage} />
          <Route path="/game/:userName/:gameId" component={GamePage} />
          <Route component={NotFoundPage} />
        </Switch>
      </div>
    </BrowserRouter>
  );
}

export default App;
