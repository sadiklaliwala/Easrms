import { AuthProvider } from '../types/common.types';
import GoogleIcon from '@mui/icons-material/Google';
import GitHubIcon from '@mui/icons-material/GitHub';
import CloudIcon from '@mui/icons-material/Cloud';
import LockIcon from '@mui/icons-material/Lock';

export const getAuthProviderIcon = (provider: AuthProvider) => {
  switch (provider) {
    case AuthProvider.Google:
      return <GoogleIcon />;
    case AuthProvider.GitHub:
      return <GitHubIcon />;
    case AuthProvider.Azure:
      return <CloudIcon />;
    case AuthProvider.Local:
      return <LockIcon />;
    default:
      return <LockIcon />;
  }
};
