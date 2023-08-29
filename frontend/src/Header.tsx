import React from 'react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { UserIcon } from './Icons';
import './Header.css';
import { userLogout, getWhoami, visible, UserData } from './User';

export const Header = () => {
  const [whoami, setWhoami]
    = useState<UserData | null>(null);
  const [whoamiLoading, setWhoamiLoading] = useState(true);
  const [dropdownOpen , setDropdownOpen ] = useState(false);
  const toggleDropDown = () => setDropdownOpen(!dropdownOpen);
  const toggleVisible = async () => {
    if (whoami != null) { visible(whoami.userName, (whoami.visible ? 0 : 1)); }
    toggleDropDown();
    const w = await getWhoami();
    setWhoamiLoading(true);
    setWhoami(w);
    setWhoamiLoading(false);
  }
  const logout = async () => {
    userLogout();
    toggleDropDown();
    const w = await getWhoami();
    setWhoamiLoading(true);
    setWhoami(w);
    setWhoamiLoading(false);
  }
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
        <div className="dropdown">
          <button onClick={toggleDropDown}>{ whoami.userName }</button>
          {dropdownOpen ? (
            <ul className="menu">
              <li className="menu-item">
                <button onClick={toggleVisible}>{whoami.visible ? "Turn Invisble" : "Turn Visible"}</button>
              </li>
              <li className="menu-item">
                <button onClick={logout}>Logout</button>
              </li>
            </ul>
          ) : null}
        </div>
      )}
    </div>
  );
}
