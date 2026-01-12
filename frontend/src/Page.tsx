import { FC, ReactNode } from 'react';
import { PageTitle } from './PageTitle';
import './Page.css'

interface Props {
  title?: string;
  children?: ReactNode;
}
export const Page: FC<Props> = ({ title, children }) => (
  <div className="Page">
    {title && <PageTitle>{title}</PageTitle>}
    {children}
  </div>
);
