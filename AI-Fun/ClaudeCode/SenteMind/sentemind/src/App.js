import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { motion } from 'framer-motion';
import Header from './components/ui/Header';
import Footer from './components/ui/Footer';
import HomePage from './pages/home/HomePage';
import ToolsPage from './pages/tools/ToolsPage';
import DualNBackGame from './components/tools/DualNBackGame';
import MindfulnessAssistant from './components/tools/MindfulnessAssistant';
import JournalingAssistant from './components/tools/JournalingAssistant';
import BreathPacer from './components/tools/BreathPacer';
import MoodTracker from './components/tools/MoodTracker';
import SoundTherapy from './components/tools/SoundTherapy';
import PremiumPage from './pages/premium/PremiumPage';
import './styles/App.css';

function App() {
  return (
    <Router>
      <div className="App">
        <Header />
        <motion.main
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.5 }}
        >
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/tools" element={<ToolsPage />} />
            <Route path="/tools/dual-n-back" element={<DualNBackGame />} />
            <Route path="/tools/mindfulness" element={<MindfulnessAssistant />} />
            <Route path="/tools/journaling" element={<JournalingAssistant />} />
            <Route path="/tools/breathing" element={<BreathPacer />} />
            <Route path="/tools/mood-tracker" element={<MoodTracker />} />
            <Route path="/tools/sound-therapy" element={<SoundTherapy />} />
            <Route path="/premium" element={<PremiumPage />} />
          </Routes>
        </motion.main>
        <Footer />
      </div>
    </Router>
  );
}

export default App;