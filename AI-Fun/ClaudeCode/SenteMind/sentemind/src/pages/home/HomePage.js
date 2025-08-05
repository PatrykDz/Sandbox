import React from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import { 
  Brain, 
  Heart, 
  Target, 
  Users, 
  Award, 
  ArrowRight, 
  CheckCircle, 
  Star,
  Zap,
  Shield,
  TrendingUp
} from 'lucide-react';

const HomePage = () => {
  const features = [
    {
      icon: Brain,
      title: "Dual N-Back Training",
      description: "Boost working memory and fluid intelligence with scientifically proven cognitive training",
      link: "/tools/dual-n-back",
      color: "from-purple-500 to-indigo-600"
    },
    {
      icon: Heart,
      title: "Mindfulness Assistant",
      description: "Guided meditation and mindfulness exercises to reduce stress and improve focus",
      link: "/tools/mindfulness",
      color: "from-green-500 to-teal-600"
    },
    {
      icon: Target,
      title: "Smart Journaling",
      description: "AI-powered journaling with personalized prompts and emotional insights",
      link: "/tools/journaling",
      color: "from-blue-500 to-cyan-600"
    },
    {
      icon: Users,
      title: "Breath Pacer",
      description: "Visual breathing exercises to activate your parasympathetic nervous system",
      link: "/tools/breathing",
      color: "from-rose-500 to-pink-600"
    },
    {
      icon: Award,
      title: "Mood Tracking",
      description: "Monitor emotional patterns and identify triggers for better mental health",
      link: "/tools/mood-tracker",
      color: "from-amber-500 to-orange-600"
    },
    {
      icon: Zap,
      title: "Sound Therapy",
      description: "Binaural beats and nature sounds for deep relaxation and focus enhancement",
      link: "/tools/sound-therapy",
      color: "from-violet-500 to-purple-600"
    }
  ];

  const benefits = [
    { text: "Improve working memory by up to 40%", verified: true },
    { text: "Reduce anxiety and stress levels", verified: true },
    { text: "Enhance emotional regulation", verified: true },
    { text: "Better sleep quality and duration", verified: true },
    { text: "Increased focus and concentration", verified: true },
    { text: "Greater self-awareness and mindfulness", verified: true }
  ];

  const stats = [
    { number: "127K+", label: "Active Users" },
    { number: "94%", label: "Report Improvement" },
    { number: "2.8M+", label: "Sessions Completed" },
    { number: "4.9/5", label: "User Rating" }
  ];

  const containerVariants = {
    hidden: { opacity: 0 },
    visible: {
      opacity: 1,
      transition: {
        staggerChildren: 0.1
      }
    }
  };

  const itemVariants = {
    hidden: { opacity: 0, y: 20 },
    visible: { opacity: 1, y: 0 }
  };

  return (
    <div className="min-h-screen">
      {/* Hero Section */}
      <section className="relative bg-gradient-to-br from-indigo-900 via-purple-900 to-pink-800 text-white py-20 overflow-hidden">
        <div className="absolute inset-0 bg-black/20"></div>
        <div className="container mx-auto px-4 relative z-10">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.8 }}
            className="text-center max-w-4xl mx-auto"
          >
            <h1 className="text-5xl md:text-7xl font-bold mb-6 bg-gradient-to-r from-white to-gray-300 bg-clip-text text-transparent">
              Transform Your Mind,
              <br />
              <span className="text-gradient">Elevate Your Life</span>
            </h1>
            <p className="text-xl md:text-2xl mb-8 text-gray-200 max-w-3xl mx-auto leading-relaxed">
              Unlock your mental potential with scientifically-backed tools designed to enhance cognitive function, reduce stress, and improve overall wellbeing in today's demanding world.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center items-center">
              <Link to="/tools" className="btn btn-primary text-lg px-8 py-4">
                Start Your Journey <ArrowRight className="w-5 h-5 ml-2" />
              </Link>
              <Link to="/premium" className="btn btn-premium text-lg px-8 py-4">
                <Star className="w-5 h-5 mr-2" />
                Unlock Premium
              </Link>
            </div>
          </motion.div>
        </div>
        
        {/* Floating Elements */}
        <div className="absolute top-20 left-10 w-20 h-20 bg-white/10 rounded-full animate-pulse"></div>
        <div className="absolute bottom-20 right-10 w-16 h-16 bg-purple-400/20 rounded-full animate-bounce"></div>
        <div className="absolute top-1/2 left-1/4 w-8 h-8 bg-pink-400/30 rounded-full animate-ping"></div>
      </section>

      {/* Stats Section */}
      <section className="py-16 bg-white">
        <div className="container mx-auto px-4">
          <motion.div
            variants={containerVariants}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            className="grid grid-cols-2 md:grid-cols-4 gap-8"
          >
            {stats.map((stat, index) => (
              <motion.div
                key={index}
                variants={itemVariants}
                className="text-center"
              >
                <div className="text-3xl md:text-4xl font-bold text-primary-600 mb-2">
                  {stat.number}
                </div>
                <div className="text-gray-600 font-medium">{stat.label}</div>
              </motion.div>
            ))}
          </motion.div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-20 bg-gray-50">
        <div className="container mx-auto px-4">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            className="text-center mb-16"
          >
            <h2 className="text-4xl md:text-5xl font-bold text-gray-800 mb-6">
              Your Complete Mental Wellness Toolkit
            </h2>
            <p className="text-xl text-gray-600 max-w-3xl mx-auto">
              Access a comprehensive suite of evidence-based tools designed by neuroscientists and mental health professionals to optimize your cognitive performance and emotional wellbeing.
            </p>
          </motion.div>

          <motion.div
            variants={containerVariants}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8"
          >
            {features.map((feature, index) => (
              <motion.div
                key={index}
                variants={itemVariants}
                className="group"
              >
                <Link to={feature.link} className="block">
                  <div className="card h-full group-hover:shadow-2xl transition-all duration-300">
                    <div className={`w-16 h-16 bg-gradient-to-br ${feature.color} rounded-xl flex items-center justify-center mb-6 group-hover:scale-110 transition-transform`}>
                      <feature.icon className="w-8 h-8 text-white" />
                    </div>
                    <h3 className="text-xl font-bold text-gray-800 mb-3 group-hover:text-primary-600 transition-colors">
                      {feature.title}
                    </h3>
                    <p className="text-gray-600 mb-4 leading-relaxed">
                      {feature.description}
                    </p>
                    <div className="flex items-center text-primary-600 font-medium group-hover:translate-x-2 transition-transform">
                      Try it now <ArrowRight className="w-4 h-4 ml-1" />
                    </div>
                  </div>
                </Link>
              </motion.div>
            ))}
          </motion.div>
        </div>
      </section>

      {/* Benefits Section */}
      <section className="py-20 bg-white">
        <div className="container mx-auto px-4">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
            <motion.div
              initial={{ opacity: 0, x: -30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
            >
              <h2 className="text-4xl md:text-5xl font-bold text-gray-800 mb-6">
                Why Mental Health Matters More Than Ever
              </h2>
              <p className="text-lg text-gray-600 mb-8 leading-relaxed">
                In our increasingly stressful world, maintaining mental wellness isn't just important—it's essential. Modern life demands peak cognitive performance while bombarding us with unprecedented levels of information and pressure.
              </p>
              <div className="space-y-4">
                {benefits.map((benefit, index) => (
                  <motion.div
                    key={index}
                    initial={{ opacity: 0, x: -20 }}
                    whileInView={{ opacity: 1, x: 0 }}
                    viewport={{ once: true }}
                    transition={{ delay: index * 0.1 }}
                    className="flex items-start space-x-3"
                  >
                    <CheckCircle className="w-6 h-6 text-green-500 mt-0.5 flex-shrink-0" />
                    <span className="text-gray-700 font-medium">{benefit.text}</span>
                    {benefit.verified && (
                      <Shield className="w-4 h-4 text-blue-500 mt-1 flex-shrink-0" />
                    )}
                  </motion.div>
                ))}
              </div>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, x: 30 }}
              whileInView={{ opacity: 1, x: 0 }}
              viewport={{ once: true }}
              className="bg-gradient-to-br from-primary-500 to-secondary-500 rounded-2xl p-8 text-white"
            >
              <TrendingUp className="w-12 h-12 mb-6" />
              <h3 className="text-2xl font-bold mb-4">Science-Backed Results</h3>
              <p className="text-primary-100 mb-6 leading-relaxed">
                Our tools are based on peer-reviewed research from leading institutions like Harvard, Stanford, and MIT. Users report significant improvements in just 2-3 weeks of consistent practice.
              </p>
              <div className="bg-white/10 rounded-lg p-4">
                <div className="text-3xl font-bold mb-2">21 Days</div>
                <div className="text-primary-100">Average time to see measurable cognitive improvement</div>
              </div>
            </motion.div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 bg-gradient-to-r from-primary-600 to-secondary-600 text-white">
        <div className="container mx-auto px-4 text-center">
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
          >
            <h2 className="text-4xl md:text-5xl font-bold mb-6">
              Ready to Transform Your Mental Wellness?
            </h2>
            <p className="text-xl mb-8 text-primary-100 max-w-2xl mx-auto">
              Join thousands of users who have already improved their cognitive function, reduced stress, and enhanced their quality of life with SenteMind.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link to="/tools" className="btn bg-white text-primary-600 hover:bg-gray-100 text-lg px-8 py-4">
                Start Free Today
              </Link>
              <Link to="/premium" className="btn btn-premium border-2 border-white text-lg px-8 py-4">
                <Star className="w-5 h-5 mr-2" />
                Get Premium Access
              </Link>
            </div>
          </motion.div>
        </div>
      </section>
    </div>
  );
};

export default HomePage;