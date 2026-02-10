using LinkUtilities.ViewModels;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUtilities.Models
{
    internal class Pipelines : List<Pipeline>
    {
        private readonly int _maxPipelines = 10;

        public Pipelines()
        { }

        public ParallelOptions ParallelOptions { get; } = new ParallelOptions { MaxDegreeOfParallelism = Convert.ToInt32(Environment.ProcessorCount) };

        public List<CheckGameLink> CheckLinks(bool hideOkOnLinkCheck)
        {
            var checkedLinks = new List<CheckGameLink>();

            if (Count == 0)
            {
                return checkedLinks;
            }

            Parallel.ForEach(this, ParallelOptions, pipeline => pipeline.CheckLinks(hideOkOnLinkCheck));

            foreach (var pipeline in this)
            {
                checkedLinks.AddRange(pipeline.CheckedLinks);
            }

            return checkedLinks;
        }

        public void CleanUp()
        {
            if (Count == 0)
            {
                return;
            }

            foreach (var pipeline in this)
            {
                pipeline.Dispose();
            }

            Clear();
        }

        // TODO: Add constructor with a list of links and a method to check those links with the pipelines. If there are more links than pipelines, check the links in batches.

        public void Initialize(int linkCount, Game game = null)
        {
            CleanUp();

            var pipelineId = 0;

            var maxPipelines = _maxPipelines > linkCount ? linkCount : _maxPipelines;
            maxPipelines = ParallelOptions.MaxDegreeOfParallelism > maxPipelines ? maxPipelines : ParallelOptions.MaxDegreeOfParallelism;

            while (pipelineId < maxPipelines)
            {
                Add(new Pipeline(pipelineId)
                {
                    Game = game
                });

                pipelineId++;
            }
        }

        public void Initialize(Game game)
        {
            if (game.Links == null || game.Links.Count == 0)
            {
                CleanUp();
                return;
            }

            Initialize(game.Links.Count, game);

            var pipelineId = 0;

            foreach (var link in game.Links)
            {
                this[pipelineId].Links.Add(link);

                pipelineId++;

                if (pipelineId >= Count)
                {
                    pipelineId = 0;
                }
            }
        }
    }
}