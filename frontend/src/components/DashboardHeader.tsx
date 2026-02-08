import { useEffect, useState } from 'react';
import { Box, Button, Chip, IconButton, Stack, TextField, Tooltip, Typography } from '@mui/material';
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded';
import AdminPanelSettingsRoundedIcon from '@mui/icons-material/AdminPanelSettingsRounded';

interface DashboardHeaderProps {
  username?: string;
  clienteId?: number | null;
  onLogout: () => void;
  isAdmin?: boolean;
  onNavigateAdmin?: () => void;
  onChangeClienteId: (value: number | null) => void;
}

const DashboardHeader = ({
  username,
  clienteId,
  onLogout,
  isAdmin = false,
  onNavigateAdmin,
  onChangeClienteId,
}: DashboardHeaderProps) => {
  const [localClienteId, setLocalClienteId] = useState(() => (clienteId ? String(clienteId) : ''));

  useEffect(() => {
    setLocalClienteId(clienteId ? String(clienteId) : '');
  }, [clienteId]);

  const handleClienteBlur = () => {
    const parsed = Number(localClienteId);
    onChangeClienteId(localClienteId.trim().length === 0 || Number.isNaN(parsed) ? null : parsed);
  };

  const greeting = username ? `Bienvenido, ${username}` : 'Home banking';

  return (
    <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={2}>
      <Box>
        <Typography variant="h3" gutterBottom className="greeting-title">
          {greeting}
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Consultá el saldo de la cuenta principal y monitoreá los últimos consumos autorizados.
        </Typography>
      </Box>
      <Stack direction="row" spacing={2} alignItems="center">
        <TextField
          label="Cliente"
          size="small"
          value={localClienteId}
          onChange={(event) => setLocalClienteId(event.target.value)}
          onBlur={handleClienteBlur}
          onKeyDown={(event) => {
            if (event.key === 'Enter') {
              handleClienteBlur();
            }
          }}
          placeholder="Ej: 1"
          sx={{ minWidth: 120 }}
        />
        {isAdmin && onNavigateAdmin && (
          <Button
            variant="outlined"
            color="primary"
            startIcon={<AdminPanelSettingsRoundedIcon />}
            onClick={onNavigateAdmin}
          >
            Panel admin
          </Button>
        )}
        <Chip color="secondary" label="Sistema bancario" variant="filled" />
        <Tooltip title="Cerrar sesión">
          <IconButton onClick={onLogout} color="error">
            <LogoutRoundedIcon />
          </IconButton>
        </Tooltip>
      </Stack>
    </Stack>
  );
};

export default DashboardHeader;
