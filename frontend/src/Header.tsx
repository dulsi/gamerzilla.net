import React from 'react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { UserIcon } from './Icons';
import './Header.css';
import { getWhoami, UserData } from './User';

export const Header = () => {
  const [whoami, setWhoami]
    = useState<UserData | null>(null);
  const [whoamiLoading, setWhoamiLoading] = useState(true);
  useEffect(() => {
    const doGetWhoami = async () => {
      if (whoamiLoading)
      {
        const w = await getWhoami();
        setWhoami(w);
        setWhoamiLoading(false);
      }
    };
    doGetWhoami();
  });
  return (
    <div className="Header">
      <Link to="/" className="Headername">Gamerzilla</Link>
      {(whoami == null) || (whoami.userName === "") ? (
      <Link to="/signin" className="Signin">
          <UserIcon />
          <span>Sign In</span>
        </Link>
      ) : (
        <span>{ whoami.userName }</span>
      )}
    </div>
  );
}