using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Addons.CommandsExtension.Entities;
using Discord.Commands;

namespace Discord.Addons.CommandsExtension
{
    public static class CommandServiceExtension
    {
        public static CommandServiceInfo GetCommandServiceInfo(this CommandService commandService, string command)
        {

            var commandInfo = commandService.Search(command).Commands.FirstOrDefault().Command;
            var module = commandInfo.Module;
            var aliases = string.Join(", ", commandInfo.Aliases);
            var parameters = string.Join(", ", commandInfo.GetCommandParameters());
            return new CommandServiceInfo(commandInfo, module, aliases, parameters);
        }


        public static Embed GetDefaultHelpEmbed(this CommandService commandService, string command, string prefix)
        {
            EmbedBuilder helpEmbedBuilder;
            var commandModules = commandService.GetModulesWithCommands();
            var moduleMatch = commandModules.FirstOrDefault(m => m.Name == command || m.Aliases.Contains(command));

            if (string.IsNullOrEmpty(command))
            {
                helpEmbedBuilder = commandService.GenerateHelpCommandEmbed();
            }
            else if (moduleMatch != null)
            {
                helpEmbedBuilder = commandService.GenerateSpecificModuleHelpEmbed(moduleMatch);
            }
            else
            {
                helpEmbedBuilder = GenerateSpecificCommandHelpEmbed(commandService, command, prefix);
            }

            helpEmbedBuilder.WithFooter(GenerateUsageFooterMessage(prefix));
            return helpEmbedBuilder.Build();
        }

        private static string GenerateUsageFooterMessage(string botPrefix)
         => $"Use {botPrefix}help [command module] or {botPrefix}help [command name] for more information.";

        private static IEnumerable<ModuleInfo> GetModulesWithCommands(this CommandService commandService)
            => commandService.Modules.Where(module => module.Commands.Count > 0);

        private static EmbedBuilder GenerateSpecificCommandHelpEmbed(this CommandService commandService, string command, string prefix)
        {

            //TODO: This won't allow commands that ends with a number
            var isNumeric = int.TryParse(command[command.Length - 1].ToString(), out var pageNum);

            if (isNumeric)
                command = command.Substring(0, command.Length - 2);
            else
                pageNum = 1;

            var helpEmbedBuilder = new EmbedBuilder();
            var commandSearchResult = commandService.Search(command);

            var commandsInfoWeNeed = new List<CommandInfo>();

            if (commandSearchResult.IsSuccess)
            {
                foreach (var c in commandSearchResult.Commands) commandsInfoWeNeed.Add(c.Command);
            }
            else
            {
                var commandModulesList = commandService.Modules.ToList();
                foreach (var c in commandModulesList) commandsInfoWeNeed.AddRange(c.Commands.Where(h => string.Equals(h.Name, command, StringComparison.CurrentCultureIgnoreCase)));
            }

            if (pageNum > commandsInfoWeNeed.Count || pageNum <= 0)
                pageNum = 1;


            if (commandsInfoWeNeed.Count <= 0)
            {
                helpEmbedBuilder.WithTitle("Command not found");
                return helpEmbedBuilder;
            }

            var commandInformation = commandsInfoWeNeed[pageNum - 1].GetCommandInfo(prefix);



            helpEmbedBuilder.WithDescription(commandInformation);

            if (commandsInfoWeNeed.Count >= 2)
                helpEmbedBuilder.WithTitle($"Variant {pageNum}/{commandsInfoWeNeed.Count}.\n" +
                                "_______\n");

            return helpEmbedBuilder;
        }

        private static EmbedBuilder GenerateSpecificModuleHelpEmbed(this CommandService commandService, ModuleInfo module)
        {
            var helpEmbedBuilder = new EmbedBuilder();
            helpEmbedBuilder.AddField(module.GetModuleName(), module.GetModuleInfo());
            return helpEmbedBuilder;
        }

        private static EmbedBuilder GenerateHelpCommandEmbed(this CommandService commandService)
        {
            var helpEmbedBuilder = new EmbedBuilder();
            var commandModules = commandService.GetModulesWithCommands();
            helpEmbedBuilder.WithTitle("How can I help you?");

            foreach (var module in commandModules)
            {
                helpEmbedBuilder.AddField(module.GetModuleName(), module.GetModuleInfo());
            }
            return helpEmbedBuilder;
        }
    }
}
