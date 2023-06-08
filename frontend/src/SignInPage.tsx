import { ChangeEvent, FC, FormEvent, useState, useEffect } from 'react';
import {
  RouteComponentProps,
  withRouter,
  Link
  } from 'react-router-dom';
import { Header } from './Header';
import { Page } from './Page';
import { userLogin, UserData, canRegister } from './User';

export const SignInPage: FC<RouteComponentProps> = ({
  history,
  location,
}) => {
  const [canregister, setCanregister]
    = useState<boolean | null>(null);
  const [canregisterLoading, setCanregisterLoading] = useState(true);
  useEffect(() => {
    const doGetCanregister = async () => {
      if (canregisterLoading)
      {
        const b = await canRegister();
        setCanregister(b);
        setCanregisterLoading(false);
      }
    };
    doGetCanregister();
  });

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
    window.location.assign(window.location.protocol + '//' + window.location.hostname + ':' + window.location.port + '/trophy');
  };

  return <Page title="Sign In">
    <form onSubmit={handleLoginSubmit}>
      <div>Username: <input type="text" name="username" value={username} onChange={handleUsernameInputChange} /></div>
      <div>Password: <input type="password" name="password" value={password} onChange={handlePasswordInputChange} /></div>
      <div><button type="submit">Login</button>
        {(canregister === true) ? (<Link to="/register" className="Signin">Register</Link>):(<span></span>)}
      </div>
    </form>
  </Page>;
}

export const SignInPageWithRouter = withRouter(SignInPage);
