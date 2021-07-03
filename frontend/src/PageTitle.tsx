import { ReactChild, ReactChildren } from 'react';
import './PageTitle.css';

interface Props {
  children?: ReactChild | ReactChildren;
}

export const PageTitle = ( {children}: Props ) => (
  <h2 className="PageTitle">{children}</h2>
)
