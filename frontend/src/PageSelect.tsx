import { FC } from 'react';
import { Link } from 'react-router-dom';
import './PageSelect.css';


interface Props {
  userName: string
  currentPage: number;
  totalPages: number;
  setGameListLoading: () => void;
}

export const PageSelect: FC<Props> = ({userName, currentPage, totalPages, setGameListLoading}) => (
<div className="Pagination">&nbsp;
  <div className="Prev">{currentPage > 0 && <Link to={`/games/${userName}/${currentPage - 1}`} onClick={setGameListLoading} >Prev</Link> }</div>
  <div className="Next">{currentPage + 1 < totalPages && <Link to={`/games/${userName}/${currentPage + 1}`} onClick={setGameListLoading}  >Next</Link> }</div>
</div>
);
