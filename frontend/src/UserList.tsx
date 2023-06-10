import { FC } from 'react';
import { Link } from 'react-router-dom';
import { UserData, approve } from './User';
import './UserList.css';

interface Props {
  data: UserData[];
}
export const UserList: FC<Props> = ({data}) => {
  const handleApprove = async (userName : string) => {
    await approve(userName);
    window.location.assign(window.location.protocol + '//' + window.location.hostname + '/trophy');
  };

  return <div className="UserList">
    {data.map(user => (
      <div key={user.userName} className="UserListItem UserListRow">
        <div className="UserListColumn UserListName"><Link to={`/games/${user.userName}`}>{user.userName}</Link></div>
        <div className="UserListColumn UserListAction">
          {user.canApprove ? (
            <button onClick={event => handleApprove(user.userName)}>Approve</button>
          ) : (
            <div>&nbsp;</div>
          )}
        </div>
      </div>
    ))}
  </div>;
}
