import { FC } from 'react';
import { PageTitle } from './PageTitle';
import './Page.css';

interface Props {
  title?: string;
}
export const Page: FC<Props> = ({ title, children }) => (
  <div className="Page">
    {title && <PageTitle>{title}</PageTitle>}
    {children}
  </div>
);
