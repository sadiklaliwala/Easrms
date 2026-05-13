import { Box } from '@mui/material';
import { Outlet } from 'react-router-dom';
import AppSidebar from '../AppSidebar/AppSidebar';
import AppTopbar from '../AppTopbar/AppTopbar';

const SIDEBAR_WIDTH = 240;
const TOPBAR_HEIGHT = 64;

const AppLayout = () => {
  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      
      {/* Sidebar */}
      <AppSidebar width={SIDEBAR_WIDTH} topbarHeight={TOPBAR_HEIGHT} />

      {/* Main Area */}
      <Box sx={{ display: 'flex', flexDirection: 'column', flexGrow: 1, ml: `${SIDEBAR_WIDTH}px` }}>
        
        {/* Topbar */}
        <AppTopbar height={TOPBAR_HEIGHT} sidebarWidth={SIDEBAR_WIDTH} />

        {/* Page Content */}
        <Box
          component="main"
          sx={{
            flexGrow: 1,
            mt: `${TOPBAR_HEIGHT}px`,
            p: 3,
            overflow: 'auto',
          }}
        >
          <Outlet />
        </Box>

      </Box>
    </Box>
  );
};

export default AppLayout;