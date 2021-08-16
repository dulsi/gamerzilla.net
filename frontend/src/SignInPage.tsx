import { ChangeEvent, FC, FormEvent, useState } from 'react';
import {
  RouteComponentProps,
  withRouter,
  } from 'react-router-dom';
import { Page } from './Page';
import { userLogin, UserData } from './User';

export const SignInPage: FC<RouteComponentProps> = ({
  history,
  location,
}) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleUsernameInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    setUsername(e.currentTarget.value);
  };
  const handlePasswordInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    setPassword(e.currentTarget.value);
  };
  const handleLoginSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const u = await userLogin(username, password);
  };

  return <Page title="Sign In">
    <form onSubmit={handleLoginSubmit}>
      <div><input type="text" name="username" value={username} onChange={handleUsernameInputChange} /></div>
      <div><input type="password" name="password" value={password} onChange={handlePasswordInputChange} /></div>
      <div><button type="submit">Login</button></div>
    </form>
  </Page>;
}

export const SignInPageWithRouter = withRouter(SignInPage);
