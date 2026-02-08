import { Avatar, Box, Chip, Paper, Stack, Typography } from '@mui/material';
import ShieldRoundedIcon from '@mui/icons-material/ShieldRounded';
import ScheduleRoundedIcon from '@mui/icons-material/ScheduleRounded';

interface SessionCardProps {
  username?: string;
  roles: string[];
  clienteId?: number | null;
  expiresAt?: string;
}

const SessionCard = ({ username, roles, clienteId, expiresAt }: SessionCardProps) => (
  <Paper elevation={0} className="session-paper">
    <Stack direction={{ xs: 'column', sm: 'row' }} spacing={3} alignItems="center">
      <Avatar sx={{ bgcolor: '#37b7c3', width: 56, height: 56 }}>{username ? username[0].toUpperCase() : '?'}</Avatar>
      <Box flex={1}>
        <Typography variant="h6">{username ?? 'Sesión bancaria'}</Typography>
        <Typography variant="body2" color="text.secondary">
          {roles.length ? roles.join(', ') : 'Sin roles asignados'}
        </Typography>
      </Box>
      <Stack direction="row" spacing={1} flexWrap="wrap" justifyContent={{ xs: 'center', sm: 'flex-end' }}>
        <Chip
          icon={<ShieldRoundedIcon />}
          label={clienteId ? `Cliente ${clienteId}` : 'Cliente no informado'}
          color="info"
          variant="outlined"
        />
        {expiresAt && (
          <Chip
            icon={<ScheduleRoundedIcon />}
            label={`Expira ${new Date(expiresAt).toLocaleString()}`}
            color="secondary"
            variant="outlined"
          />
        )}
      </Stack>
    </Stack>
  </Paper>
);

export default SessionCard;
